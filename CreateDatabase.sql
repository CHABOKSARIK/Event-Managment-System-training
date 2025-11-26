
CREATE TABLE IF NOT EXISTS Должность (
    Код_должности INTEGER PRIMARY KEY AUTOINCREMENT,
    Название_должности TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS Пользователь (
    Код_пользователя INTEGER PRIMARY KEY AUTOINCREMENT,
    Имя TEXT NOT NULL,
    Фамилия TEXT NOT NULL,
    Должность INTEGER NOT NULL,
    Логин TEXT NOT NULL UNIQUE,
    Пароль TEXT NOT NULL,
    FOREIGN KEY (Должность) REFERENCES Должность(Код_должности)
);
INSERT OR IGNORE INTO Должность (Название_должности) VALUES 
('Администратор'),
('Координатор'),
('Клиент'),
('Поставщик');

-- Вставка тестовых пользователей (пароль: 123456)
INSERT OR IGNORE INTO Пользователь (Имя, Фамилия, Должность, Логин, Пароль) VALUES
('Анна', 'Иванова', 1, 'admin', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92'),
('Петр', 'Сидоров', 2, 'coordinator', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92'),
('Мария', 'Петрова', 3, 'client', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92'),
('Олег', 'Кузнецов', 4, 'supplier', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92');