USE [MyAddressBook];
GO

-- DROP TABLE [Contact];

CREATE TABLE [Contact] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max),
	[Email] nvarchar(max),
    [Phone] nvarchar(max),
	[Address] nvarchar(max),
    [PictureName] nvarchar(max),
	[SIN_Number] nvarchar(max),
);
GO