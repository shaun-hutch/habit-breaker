-- ============================================================
-- HabitBreaker Database Initialisation Script
-- NOTE: EF Core handles migrations at runtime via db.Database.MigrateAsync()
--       in Program.cs. This script is a reference/manual bootstrap only.
-- ============================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'HabitBreaker')
BEGIN
    CREATE DATABASE HabitBreaker;
END;
GO

USE HabitBreaker;
GO

-- ─── ASP.NET Identity Tables ─────────────────────────────────────────────────
-- These are created by EF migrations. Listed here for reference.
-- AspNetRoles, AspNetUsers, AspNetRoleClaims, AspNetUserClaims,
-- AspNetUserLogins, AspNetUserRoles, AspNetUserTokens

-- ─── Application Tables ───────────────────────────────────────────────────────

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Habits')
BEGIN
    CREATE TABLE [dbo].[Habits] (
        [Id]                 INT           NOT NULL IDENTITY(1,1),
        [Name]               NVARCHAR(200) NOT NULL,
        [Description]        NVARCHAR(2000) NOT NULL DEFAULT '',
        [DefaultDurationDays] INT          NOT NULL DEFAULT 66,
        CONSTRAINT [PK_Habits] PRIMARY KEY ([Id])
    );

    -- Seed the initial "Better Eating" habit
    INSERT INTO [dbo].[Habits] ([Name], [Description], [DefaultDurationDays])
    VALUES ('Better Eating', 'Track your daily food and drink intake to build healthier eating habits.', 66);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserHabits')
BEGIN
    CREATE TABLE [dbo].[UserHabits] (
        [Id]           INT      NOT NULL IDENTITY(1,1),
        [UserId]       NVARCHAR(450) NOT NULL,
        [HabitId]      INT      NOT NULL,
        [StartDate]    DATE     NOT NULL,
        [DurationDays] INT      NOT NULL DEFAULT 66,
        [IsActive]     BIT      NOT NULL DEFAULT 1,
        CONSTRAINT [PK_UserHabits] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserHabits_Users] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserHabits_Habits] FOREIGN KEY ([HabitId]) REFERENCES [Habits]([Id]) ON DELETE NO ACTION
    );
    CREATE INDEX [IX_UserHabits_UserId_HabitId_IsActive] ON [dbo].[UserHabits] ([UserId], [HabitId], [IsActive]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FoodLogs')
BEGIN
    CREATE TABLE [dbo].[FoodLogs] (
        [Id]           INT      NOT NULL IDENTITY(1,1),
        [UserHabitId]  INT      NOT NULL,
        [LogDate]      DATE     NOT NULL,
        [CreatedAt]    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt]    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_FoodLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FoodLogs_UserHabits] FOREIGN KEY ([UserHabitId]) REFERENCES [UserHabits]([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_FoodLogs_UserHabitId_LogDate] UNIQUE ([UserHabitId], [LogDate])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MealEntries')
BEGIN
    CREATE TABLE [dbo].[MealEntries] (
        [Id]          INT           NOT NULL IDENTITY(1,1),
        [FoodLogId]   INT           NOT NULL,
        [MealType]    INT           NOT NULL,   -- 0=Breakfast,1=MorningTea,2=Lunch,3=Dinner,4=Supper
        [Description] NVARCHAR(2000) NOT NULL DEFAULT '',
        [Rating]      INT           NOT NULL,   -- 0=Healthy,1=Unhealthy
        [CreatedAt]   DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_MealEntries] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MealEntries_FoodLogs] FOREIGN KEY ([FoodLogId]) REFERENCES [FoodLogs]([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PushSubscriptions')
BEGIN
    CREATE TABLE [dbo].[PushSubscriptions] (
        [Id]        INT           NOT NULL IDENTITY(1,1),
        [UserId]    NVARCHAR(450) NOT NULL,
        [Endpoint]  NVARCHAR(1000) NOT NULL,
        [P256dhKey] NVARCHAR(500) NOT NULL,
        [AuthKey]   NVARCHAR(500) NOT NULL,
        [CreatedAt] DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_PushSubscriptions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PushSubscriptions_Users] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE [dbo].[RefreshTokens] (
        [Id]         INT           NOT NULL IDENTITY(1,1),
        [UserId]     NVARCHAR(450) NOT NULL,
        [Token]      NVARCHAR(500) NOT NULL,
        [ExpiresAt]  DATETIME2     NOT NULL,
        [IsRevoked]  BIT           NOT NULL DEFAULT 0,
        [CreatedAt]  DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [UQ_RefreshTokens_Token] UNIQUE ([Token]),
        CONSTRAINT [FK_RefreshTokens_Users] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE
    );
END;
GO
