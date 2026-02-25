use master
go

create database [Project_SP26_G3]
go

use [Project_SP26_G3]
go

/****** Object:  Table [dbo].[Role]    Script Date: 03/11/2022 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create table [dbo].[Roles](
	[RoleId] [int] identity(1,1) not null,
	[RoleName] [nvarchar](50) null,
	primary key clustered([RoleId] asc) with(
	pad_index = off, statistics_norecompute = off, ignore_dup_key = off, allow_row_locks = on, allow_page_locks = on) on [primary]
) on [primary]
go
/****** Object:  Table [dbo].[Person]    Script Date: 03/11/2022 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create table [dbo].[Persons](
	[PersonId] [int] identity(1,1) not null,
	[PersonName] [nvarchar](50) not null,
	[Password] [varchar](255) not null,
	[Birthdate] [smalldatetime] not null,
	[Address] [nvarchar](255) null,
	[Phone] [varchar](12) null,
	[Status] bit not null check([status] in (0,1)),
	[RoleId] [int] not null,
	primary key clustered([PersonId] asc) with(
	pad_index = off, statistics_norecompute = off, ignore_dup_key = off, allow_row_locks = on, allow_page_locks = on) on [primary],
	constraint [uq_PersonName] unique nonclustered([PersonName]) with (
	pad_index = off, statistics_norecompute = off, ignore_dup_key = off, allow_row_locks = on, allow_page_locks = on) on [primary],
	constraint [fk_role_person] foreign key ([RoleId]) references [Roles]([RoleId])
	on update cascade
	on delete cascade
) on [primary]
go
/****** Object:  Table [dbo].[Warehouse]    Script Date: 03/11/2022 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create table [dbo].[Warehouses](
	[WarehouseId] [int] identity(1,1) not null,
	[WarehouseName] [varchar](50) not null,
	[Size] [int] not null,
	[Status] bit not null check([status] in (0,1)),
	primary key clustered([WarehouseId] asc) with(
	pad_index = off, statistics_norecompute = off, ignore_dup_key = off, allow_row_locks = on, allow_page_locks = on) on [primary],
	constraint [uq_Wname] unique nonclustered ([WarehouseName]) with(
	pad_index = off, statistics_norecompute = off, ignore_dup_key = off, allow_row_locks = on, allow_page_locks = on) on [primary],
) on [primary]
go
/****** Object:  Table [dbo].[Categories]    Script Date: 03/11/2022 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create table [dbo].[Categories](
	[CatId] [int] identity(1,1) not null,
	[CatName] [varchar](50) null,
	primary key clustered([Catid] asc) with(
	pad_index = off, statistics_norecompute = off, ignore_dup_key = off, allow_row_locks = on, allow_page_locks = on) on [primary]
) on [primary]
go
/****** Object:  Table [dbo].[Products]    Script Date: 03/11/2022 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create table [Products](
	[ProductId] [int] identity(1,1) not null,
	[ProductCode] [varchar](20) not null,
	[ProductName] [varchar](50) null,
	[Quantity] [int] null,
	[Sender] [nvarchar](50) null,
	[Location] [varchar](50) null,
	[CatId] [int] not null,
	[DateIn] [smalldatetime] not null,
	[DateOut] [smalldatetime] null,
	[Status] bit  not null check([status] in (0,1)),
	primary key clustered([ProductId] asc) with(
	pad_index = off, statistics_norecompute = off, ignore_dup_key = off, allow_row_locks = on, allow_page_locks = on) on [primary],
	constraint [uq_ProductCode] unique nonclustered (ProductCode) with(
	pad_index = off, statistics_norecompute = off, ignore_dup_key = off, allow_row_locks = on, allow_page_locks = on) on [primary],
	constraint [fk_product_categories] foreign key ([CatId]) references [Categories]([CatId]),
	constraint [fk_product_person] foreign key ([Sender]) references [Persons]([PersonName]),
	constraint [fk_product_warehouse] foreign key ([Location]) references [Warehouses]([WarehouseName])
) on [primary]
go
/****** Query: INSERT *****/
SET IDENTITY_INSERT [dbo].[Roles] on
/***** Insert here *****/
SET IDENTITY_INSERT [dbo].[Roles] off
go
SET IDENTITY_INSERT [dbo].[Persons] on
/***** Insert here *****/
SET IDENTITY_INSERT [dbo].[Persons] off
go
SET IDENTITY_INSERT [dbo].[Warehouses] on
/***** Insert here *****/
SET IDENTITY_INSERT [dbo].[Warehouses] off
go
SET IDENTITY_INSERT [dbo].[Categories] on
/***** Insert here *****/
SET IDENTITY_INSERT [dbo].[Categories] off
go
SET IDENTITY_INSERT [dbo].[Products] on
/***** Insert here *****/
SET IDENTITY_INSERT [dbo].[Products] off
go
