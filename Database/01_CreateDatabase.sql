IF DB_ID(N'PharmacyDB') IS NULL
BEGIN
    CREATE DATABASE PharmacyDB;
END
GO

USE PharmacyDB;
GO

IF OBJECT_ID(N'dbo.Medicines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Medicines
    (
        MedicineId INT IDENTITY(1,1)
            CONSTRAINT PK_Medicines PRIMARY KEY,

        MedicineName NVARCHAR(150) NOT NULL,

        Manufacturer NVARCHAR(150) NULL,

        BatchNo NVARCHAR(50) NULL,

        ExpiryDate DATE NULL,

        PurchasePrice DECIMAL(12,2) NOT NULL
            CONSTRAINT CK_Medicines_PurchasePrice
            CHECK (PurchasePrice >= 0),

        SellingPrice DECIMAL(12,2) NOT NULL
            CONSTRAINT CK_Medicines_SellingPrice
            CHECK (SellingPrice >= 0),

        Quantity INT NOT NULL
            CONSTRAINT DF_Medicines_Quantity
            DEFAULT 0
            CONSTRAINT CK_Medicines_Quantity
            CHECK (Quantity >= 0),

        IsActive BIT NOT NULL
            CONSTRAINT DF_Medicines_IsActive
            DEFAULT 1,

        CreatedAt DATETIME2 NOT NULL
            CONSTRAINT DF_Medicines_CreatedAt
            DEFAULT SYSDATETIME()
    );

    CREATE INDEX IX_Medicines_MedicineName
        ON dbo.Medicines(MedicineName);

    CREATE INDEX IX_Medicines_ExpiryDate
        ON dbo.Medicines(ExpiryDate);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Medicines
)
BEGIN
    INSERT INTO dbo.Medicines
    (
        MedicineName,
        Manufacturer,
        BatchNo,
        ExpiryDate,
        PurchasePrice,
        SellingPrice,
        Quantity
    )
    VALUES
    ('Paracetamol 500mg', 'Example Pharma', 'PCM001',
        DATEADD(MONTH, 18, CAST(GETDATE() AS DATE)), 15.00, 25.00, 100),

    ('Cetirizine 10mg', 'Example Pharma', 'CTZ001',
        DATEADD(MONTH, 12, CAST(GETDATE() AS DATE)), 20.00, 35.00, 50),

    ('Amoxicillin 500mg', 'Example Pharma', 'AMX001',
        DATEADD(MONTH, 10, CAST(GETDATE() AS DATE)), 50.00, 80.00, 25);
END
GO

SELECT * FROM dbo.Medicines;
GO
