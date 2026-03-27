using HabitBreaker.API.Controllers;
using HabitBreaker.API.Data.Repositories;
using HabitBreaker.API.Dtos;
using HabitBreaker.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace HabitBreaker.Tests;

public class HabitsControllerTests
{
    private static HabitsController CreateController(
        Mock<IUserHabitRepository>? repo = null,
        string userId = "user-1")
    {
        repo ??= new Mock<IUserHabitRepository>();
        var controller = new HabitsController(repo.Object);
        var claims = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId)], "mock"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claims }
        };
        return controller;
    }

    [Fact]
    public async Task GetAll_ReturnsAllHabits()
    {
        var repo = new Mock<IUserHabitRepository>();
        repo.Setup(r => r.GetAllHabitsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Habit { Id = 1, Name = "Better Eating", DefaultDurationDays = 66 }]);

        var controller = CreateController(repo);
        var result = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var habits = Assert.IsAssignableFrom<IEnumerable<HabitDto>>(ok.Value);
        Assert.Single(habits);
    }

    [Fact]
    public async Task Enroll_NewHabit_ReturnsCreated()
    {
        var repo = new Mock<IUserHabitRepository>();
        repo.Setup(r => r.GetActiveUserHabitAsync("user-1", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserHabit?)null);

        var created = new UserHabit
        {
            Id = 1,
            UserId = "user-1",
            HabitId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DurationDays = 66,
            Habit = new Habit { Id = 1, Name = "Better Eating", DefaultDurationDays = 66 }
        };
        repo.Setup(r => r.EnrollAsync(It.IsAny<UserHabit>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var controller = CreateController(repo);
        var result = await controller.Enroll(new EnrollRequest(1, null), CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task Enroll_AlreadyEnrolled_ReturnsConflict()
    {
        var repo = new Mock<IUserHabitRepository>();
        repo.Setup(r => r.GetActiveUserHabitAsync("user-1", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserHabit { Id = 1, Habit = new Habit() });

        var controller = CreateController(repo);
        var result = await controller.Enroll(new EnrollRequest(1, null), CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task GetMine_ReturnsUserHabits()
    {
        var repo = new Mock<IUserHabitRepository>();
        repo.Setup(r => r.GetUserHabitsAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([new UserHabit
            {
                Id = 1,
                UserId = "user-1",
                HabitId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DurationDays = 66,
                Habit = new Habit { Id = 1, Name = "Better Eating", DefaultDurationDays = 66 }
            }]);

        var controller = CreateController(repo);
        var result = await controller.GetMine(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}
