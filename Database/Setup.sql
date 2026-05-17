-- SQL Server Database Setup for E-Commerce System

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ECommerceDB')
BEGIN
    CREATE DATABASE ECommerceDB;
END;
GO

USE ECommerceDB;
GO

-- Users Table
CREATE TABLE [Users] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Username] NVARCHAR(100) NOT NULL UNIQUE,
    [Email] NVARCHAR(255) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [Role] INT NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [PhoneNumber] NVARCHAR(20),
    [Address] NVARCHAR(MAX),
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2
);

CREATE INDEX IX_Users_Email ON [Users]([Email]);
CREATE INDEX IX_Users_Username ON [Users]([Username]);

-- Categories Table
CREATE TABLE [Categories] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Name] NVARCHAR(150) NOT NULL,
    [Description] NVARCHAR(MAX),
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2
);

-- Products Table
CREATE TABLE [Products] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [Name] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [Price] DECIMAL(18,2) NOT NULL,
    [StockQuantity] INT NOT NULL,
    [Status] INT NOT NULL DEFAULT 1,
    [SKU] NVARCHAR(50) NOT NULL UNIQUE,
    [ImageUrl] NVARCHAR(MAX),
    [CategoryId] INT NOT NULL,
    [Rating] DECIMAL(3,2) NOT NULL DEFAULT 0,
    [TotalReviews] INT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    FOREIGN KEY ([CategoryId]) REFERENCES [Categories]([Id]) ON DELETE NO ACTION
);

CREATE INDEX IX_Products_SKU ON [Products]([SKU]);
CREATE INDEX IX_Products_CategoryId ON [Products]([CategoryId]);
CREATE INDEX IX_Products_Status ON [Products]([Status]);

-- Orders Table
CREATE TABLE [Orders] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [UserId] INT NOT NULL,
    [OrderNumber] NVARCHAR(50) NOT NULL UNIQUE,
    [Status] INT NOT NULL DEFAULT 1,
    [OrderDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ShippedDate] DATETIME2,
    [DeliveredDate] DATETIME2,
    [TotalAmount] DECIMAL(18,2) NOT NULL,
    [ShippingAddress] NVARCHAR(MAX),
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE NO ACTION
);

CREATE INDEX IX_Orders_UserId ON [Orders]([UserId]);
CREATE INDEX IX_Orders_Status ON [Orders]([Status]);
CREATE INDEX IX_Orders_OrderNumber ON [Orders]([OrderNumber]);

-- OrderItems Table
CREATE TABLE [OrderItems] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [OrderId] INT NOT NULL,
    [ProductId] INT NOT NULL,
    [Quantity] INT NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([ProductId]) REFERENCES [Products]([Id]) ON DELETE NO ACTION
);

CREATE INDEX IX_OrderItems_OrderId ON [OrderItems]([OrderId]);
CREATE INDEX IX_OrderItems_ProductId ON [OrderItems]([ProductId]);

-- Reviews Table
CREATE TABLE [Reviews] (
    [Id] INT PRIMARY KEY IDENTITY(1,1),
    [ProductId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [Rating] INT NOT NULL,
    [Title] NVARCHAR(255) NOT NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [HelpfulCount] INT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    FOREIGN KEY ([ProductId]) REFERENCES [Products]([Id]) ON DELETE CASCADE,
    FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE NO ACTION
);

CREATE INDEX IX_Reviews_ProductId ON [Reviews]([ProductId]);
CREATE INDEX IX_Reviews_UserId ON [Reviews]([UserId]);

-- Sample Data
INSERT INTO [Categories] ([Name], [Description], [IsActive])
VALUES 
    ('Electronics', 'Electronic devices and gadgets', 1),
    ('Clothing', 'Apparel and accessories', 1),
    ('Books', 'Physical and digital books', 1);

-- Views for Analytics
CREATE VIEW vw_OrderSummary AS
SELECT 
    o.[Id],
    o.[OrderNumber],
    u.[Username],
    o.[OrderDate],
    o.[TotalAmount],
    o.[Status]
FROM [Orders] o
INNER JOIN [Users] u ON o.[UserId] = u.[Id]
WHERE o.[Status] != 5; -- Exclude cancelled orders

CREATE VIEW vw_ProductPerformance AS
SELECT 
    p.[Id],
    p.[Name],
    p.[Price],
    p.[StockQuantity],
    p.[Rating],
    p.[TotalReviews],
    COUNT(oi.[Id]) as OrderCount
FROM [Products] p
LEFT JOIN [OrderItems] oi ON p.[Id] = oi.[ProductId]
GROUP BY p.[Id], p.[Name], p.[Price], p.[StockQuantity], p.[Rating], p.[TotalReviews];

PRINT 'Database setup completed successfully!';
