using HabitBreaker.API.Controllers;
using HabitBreaker.API.Data.Repositories;
using HabitBreaker.API.Dtos;
using HabitBreaker.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace HabitBreaker.Tests;

public class FoodLogControllerTests
{
    private static FoodLogController CreateController(
        Mock<IFoodLogRepository>? foodRepo = null,
        Mock<IUserHabitRepository>? habitRepo = null,
        string userId = "user-1")
    {
        foodRepo ??= new Mock<IFoodLogRepository>();
        habitRepo ??= new Mock<IUserHabitRepository>();

        var controller = new FoodLogController(foodRepo.Object, habitRepo.Object);
        var claims = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId)], "mock"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claims }
        };
        return controller;
    }

    private static List<UserHabit> UserHabits(int id, string userId) =>
        [new UserHabit { Id = id, UserId = userId, HabitId = 1, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), Habit = new Habit { Id = 1, Name = "Better Eating" } }];

    [Fact]
    public async Task GetByDate_ExistingLog_ReturnsOk()
    {
        var foodRepo = new Mock<IFoodLogRepository>();
        var habitRepo = new Mock<IUserHabitRepository>();

        habitRepo.Setup(r => r.GetUserHabitsAsync("user-1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(UserHabits(10, "user-1"));

        foodRepo.Setup(r => r.GetByDateAsync(10, new DateOnly(2026, 1, 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FoodLog { Id = 1, UserHabitId = 10, LogDate = new DateOnly(2026, 1, 1), MealEntries = [] });

        var controller = CreateController(foodRepo, habitRepo);
        var result = await controller.GetByDate(10, "2026-01-01", CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetByDate_NotFound_Returns404()
    {
        var foodRepo = new Mock<IFoodLogRepository>();
        var habitRepo = new Mock<IUserHabitRepository>();

        habitRepo.Setup(r => r.GetUserHabitsAsync("user-1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(UserHabits(10, "user-1"));

        foodRepo.Setup(r => r.GetByDateAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((FoodLog?)null);

        var controller = CreateController(foodRepo, habitRepo);
        var result = await controller.GetByDate(10, "2026-01-01", CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetByDate_UnauthorizedHabit_ReturnsForbid()
    {
        var habitRepo = new Mock<IUserHabitRepository>();
        habitRepo.Setup(r => r.GetUserHabitsAsync("user-1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync([]); // no owned habits

        var controller = CreateController(habitRepo: habitRepo);
        var result = await controller.GetByDate(99, "2026-01-01", CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Upsert_ValidRequest_ReturnsOk()
    {
        var foodRepo = new Mock<IFoodLogRepository>();
        var habitRepo = new Mock<IUserHabitRepository>();

        habitRepo.Setup(r => r.GetUserHabitsAsync("user-1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(UserHabits(10, "user-1"));

        var savedLog = new FoodLog { Id = 1, UserHabitId = 10, LogDate = new DateOnly(2026, 1, 1), MealEntries = [] };
        foodRepo.Setup(r => r.UpsertAsync(It.IsAny<FoodLog>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(savedLog);

        var controller = CreateController(foodRepo, habitRepo);
        var result = await controller.Upsert(
            new UpsertFoodLogRequest("2026-01-01", 10, [new MealEntryRequest(0, "Eggs", 0)]),
            CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetHistory_ValidRequest_ReturnsPagedResult()
    {
        var foodRepo = new Mock<IFoodLogRepository>();
        var habitRepo = new Mock<IUserHabitRepository>();

        habitRepo.Setup(r => r.GetUserHabitsAsync("user-1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(UserHabits(10, "user-1"));

        foodRepo.Setup(r => r.GetHistoryAsync(10, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), 1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync([new FoodLog { Id = 1, UserHabitId = 10, LogDate = new DateOnly(2026, 1, 1), MealEntries = [] }]);

        foodRepo.Setup(r => r.GetHistoryCountAsync(10, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

        var controller = CreateController(foodRepo, habitRepo);
        var result = await controller.GetHistory(10, null, null, 1, 20, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<FoodLogHistoryResponse>(ok.Value);
        Assert.Equal(1, response.TotalCount);
    }
}
