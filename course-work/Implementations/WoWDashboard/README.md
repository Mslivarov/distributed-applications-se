**Факултетен номер:** 2301321012  
**Име на проекта:** WoWDashboard  
**Описание:** Уеб приложение, което позволява визуализация и анализ на данни за World of Warcraft герои. Проектът използва публичния Blizzard API за извличане на информация за герои, екипировка и рейд прогресия. Данните се съхраняват в MySQL база и се визуализират чрез удобен уеб интерфейс.

## Инструкции за инсталация и стартиране
Проектът е разработен с .NET 7. Може да се наложи да инсталирате .NET 7 SDK или да актуализирате версиите на пакетите в проекта, ако използвате по-нова версия на .NET. Базата с която е свързан проекта е localhost на моята машина затова може да се наложи да обновите конекцията в appsetting.json със информацията за вашата база

## Script за базата 
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(255) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL
);
CREATE TABLE Characters (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserID INT NOT NULL,
    OriginalName VARCHAR(255) NOT NULL,
    OriginalRealm VARCHAR(255) NOT NULL,
    OriginalRegion VARCHAR(255) NOT NULL,
    Name VARCHAR(255) NOT NULL,
    Realm VARCHAR(255) NOT NULL,
    Region VARCHAR(255) NOT NULL,
    Level INT NOT NULL,
    Race VARCHAR(255) NOT NULL,
    Guild VARCHAR(255) NOT NULL,
    CharacterClass VARCHAR(255) NOT NULL,
    RaiderIoScore DOUBLE,
    AvatarUrl VARCHAR(500),
    FOREIGN KEY (UserId) REFERENCES Users(id) ON DELETE CASCADE
);

CREATE TABLE UserCharacters (
    UserId INT,
    CharacterId INT,
    PRIMARY KEY (UserId, CharacterId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (CharacterId) REFERENCES Characters(Id) ON DELETE CASCADE
);

CREATE TABLE GearItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Slot VARCHAR(255) NOT NULL,
    Name VARCHAR(255) NOT NULL,
    Rarity VARCHAR(255) NOT NULL,
    ItemLevel INT NOT NULL,
    ItemId INT NOT NULL,
    CharacterId INT,
    FOREIGN KEY (CharacterId) REFERENCES Characters(Id) ON DELETE CASCADE
);

CREATE TABLE RaidProgressions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    RaidName VARCHAR(255) NOT NULL,
    Summary TEXT NOT NULL,
    CharacterId INT,
    FOREIGN KEY (CharacterId) REFERENCES Characters(Id) ON DELETE CASCADE
);

### Изисквания
- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- MySQL Server (примерно: localhost:3306)
- Visual Studio 2022 

