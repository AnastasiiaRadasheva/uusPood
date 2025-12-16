{
    public partial class Form3 : Form // Объявление класса Form3, который наследует стандартную форму Windows Forms (Form)
    {
        private AutoDbContext _db; // Приватное поле для работы с базой данных через Entity Framework. Без него мы не сможем получать и сохранять данные.
        private Form1 _mainForm; // Ссылка на главную форму, чтобы после изменений можно было обновлять список расписаний.
        private int _scheduleId = 0; // Идентификатор расписания. 0 означает, что создается новая запись, иначе редактируем существующую.

        // Конструктор формы для создания нового расписания
        public Form3(Form1 mainForm, AutoDbContext db)
        {
            InitializeComponent(); // Инициализация визуальных компонентов формы. Без неё форма не будет отображаться корректно.

            _mainForm = mainForm; // Сохраняем ссылку на главную форму
            _db = db; // Сохраняем ссылку на контекст базы данных

            LoadCombos(); // Загружаем все данные для комбобоксов (машины, услуги, работники)
            // Без этой функции комбобоксы будут пустыми и пользователь не сможет выбрать значения

            startPicker.MinDate = DateTime.Today; // Минимальная дата для выбора — сегодня, чтобы нельзя было выбрать прошлую дату
            timePicker.MinDate = DateTime.Now; // Минимальное время для выбора — текущее, чтобы нельзя было выбрать прошлое время
        }

        // Конструктор формы для редактирования существующего расписания
        public Form3(Form1 mainForm, AutoDbContext db, int scheduleId) : this(mainForm, db)
        {
            _scheduleId = scheduleId; // Сохраняем ID расписания, которое редактируем
            LoadSchedule(); // Загружаем данные расписания в форму
            // Без этой функции форма при редактировании будет пустой, пользователь не увидит текущие значения
        }

        // Функция для заполнения комбобоксов из базы данных
        private void LoadCombos()
        {
            // Загрузка списка машин
            autoCombo.DataSource = _db.Cars.ToList(); // Получаем все машины из базы
            autoCombo.DisplayMember = "RegistrationNumber"; // Отображаем пользователю регистрационный номер
            autoCombo.ValueMember = "Id"; // В коде используем ID машины

            // Загрузка списка услуг
            serviceCombo.DataSource = _db.Services.ToList(); // Получаем все услуги из базы
            serviceCombo.DisplayMember = "Name"; // Отображаем пользователю название услуги
            serviceCombo.ValueMember = "Id"; // В коде используем ID услуги

            // Загрузка списка работников
            workCOMBO.DataSource = _db.Workers.ToList(); // Получаем всех работников
            workCOMBO.DisplayMember = "FullName"; // Отображаем полное имя
            workCOMBO.ValueMember = "Id"; // В коде используем ID работника
            // Без этой функции пользователь не сможет выбрать машину, услугу или работника
        }

        // Функция для подгрузки данных конкретного расписания для редактирования
        private void LoadSchedule()
        {
            var s = _db.Schedules.First(x => x.Id == _scheduleId); // Получаем запись по ID
            // Без этой строки невозможно загрузить расписание из базы, форма будет пустой

            autoCombo.SelectedValue = s.CarId; // Устанавливаем выбранную машину
            serviceCombo.SelectedValue = s.ServiceId; // Устанавливаем выбранную услугу
            workCOMBO.SelectedValue = s.WorkerId; // Устанавливаем выбранного работника

            startPicker.Value = s.StartTime.Date; // Устанавливаем дату начала
            timePicker.Value = s.StartTime; // Устанавливаем время начала
            durationUpDown.Value = (decimal)(s.EndTime - s.StartTime).TotalHours; // Устанавливаем продолжительность
            // Эти присвоения показывают пользователю текущие значения расписания, без них форма будет некорректной
        }

        private void workCOMBO_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Пустой обработчик события изменения работника
            // Здесь можно добавить логику, например, фильтр по выбранному работнику
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Пустой обработчик клика по label1
        }

        // Функция для применения локализации ко всем контролам формы
        private void ApplyResourcesToControl(Control ctrl, ComponentResourceManager res)
        {
            res.ApplyResources(ctrl, ctrl.Name); // Применяем ресурсы к текущему контролу
            foreach (Control child in ctrl.Controls) // Проходим по всем дочерним контролам
            {
                ApplyResourcesToControl(child, res); // Рекурсивно применяем ресурсы
            }
            // Без этой функции форма не будет корректно менять язык интерфейса
        }

        // Событие нажатия кнопки "Сохранить"
        private void formkoik_Click(object sender, EventArgs e)
        {
            // Проверяем, что все комбобоксы заполнены
            if (autoCombo.SelectedItem == null || serviceCombo.SelectedItem == null || workCOMBO.SelectedItem == null)
            {
                MessageBox.Show("lisa koik"); // Сообщение пользователю, если что-то не выбрано
                return; // Прерываем сохранение
            }

            // Формируем дату и время начала из двух контролов
            DateTime start = startPicker.Value.Date + timePicker.Value.TimeOfDay;
            // startPicker.Value.Date — берем только дату
            // timePicker.Value.TimeOfDay — берем только время
            // Без этого расчета невозможно корректно сохранить дату и время начала

            if (start < DateTime.Now) // Проверяем, что время начала не в прошлом
            {
                MessageBox.Show("Aeg ei saa olla minevikus"); // Сообщение пользователю
                return; // Прерываем сохранение
            }

            DateTime end = start.AddHours((double)durationUpDown.Value); // Вычисляем время окончания, прибавляя продолжительность
            // Без этой строки запись расписания будет без корректного времени окончания

            Schedule s; // Переменная для хранения записи расписания

            if (_scheduleId == 0) // Если создаем новое расписание
            {
                s = new Schedule(); // Создаем новый объект
                _db.Schedules.Add(s); // Добавляем в контекст базы
            }
            else // Если редактируем существующее
            {
                s = _db.Schedules.First(x => x.Id == _scheduleId); // Загружаем запись по ID
            }

            // Присваиваем значения записи расписания
            s.StartTime = start; // Время начала
            s.EndTime = end; // Время окончания
            s.CarId = (int)autoCombo.SelectedValue; // Машина
            s.ServiceId = (int)serviceCombo.SelectedValue; // Услуга
            s.WorkerId = (int)workCOMBO.SelectedValue; // Работник

            _db.SaveChanges(); // Сохраняем изменения в базе данных
            // Без этого изменения не сохранятся

            MessageBox.Show("Salvesta"); // Сообщаем пользователю, что запись сохранена
            _mainForm.LaeSchedule(); // Обновляем список расписаний на главной форме
            Close(); // Закрываем текущую форму
        }

        private void label2_Click(object sender, EventArgs e)
        {
            // Пустой обработчик клика по label2
        }
    }
}

// Обработчик события TextChanged для textBox3
// Вызывается КАЖДЫЙ РАЗ, когда пользователь меняет текст в textBox3
private void textBox3_TextChanged_1(object sender, EventArgs e)
{
    // try нужен, чтобы программа НЕ УПАЛА при ошибке
    // Например: null-значения, проблемы с БД и т.п.
    try
    {
        // Берём текст из textBox3
        // Trim() — убирает пробелы в начале и конце
        // ToLowerInvariant() — переводит текст в нижний регистр
        // Это нужно для поиска БЕЗ учёта регистра
        string q = textBox3.Text.Trim().ToLowerInvariant();

        // Проверяем: пустая ли строка поиска
        // Если пользователь удалил весь текст
        if (string.IsNullOrEmpty(q))
        {
            // Загружаем ВСЕ данные без фильтра
            // Если этого не сделать — таблица останется пустой
            LaeCarService();
            return; // Выходим из метода, чтобы код ниже не выполнялся
        }

        // Запрос к базе данных через Entity Framework
        var list = _db.CarServices

            // Подгружаем связанную таблицу Car
            // Если убрать Include — cs.Car будет null
            // и cs.Car.Brand вызовет ошибку
            .Include(cs => cs.Car)

            // Подгружаем связанную таблицу Service
            // Без этого cs.Service будет null
            .Include(cs => cs.Service)

            // Фильтрация данных (WHERE в SQL)
            .Where(cs =>

                // Проверяем, что машина существует (не null)
                // Без этого будет NullReferenceException
                (cs.Car != null && (

                    // Поиск по марке автомобиля
                    // ToLower() — чтобы сравнивать с q
                    cs.Car.Brand.ToLower().Contains(q)

                    // ИЛИ поиск по модели
                    || cs.Car.Model.ToLower().Contains(q)

                    // ИЛИ поиск по номеру регистрации
                    || cs.Car.RegistrationNumber.ToLower().Contains(q)
                ))

                // ИЛИ поиск по названию услуги
                // Проверка cs.Service != null — защита от ошибки
                || (cs.Service != null && cs.Service.Name.ToLower().Contains(q))

                // ИЛИ поиск по пробегу
                // Mileage — число, поэтому:
                // ToString() превращает его в текст
                || cs.Mileage.ToString().Contains(q)
            )

            // Select — выбираем, ЧТО именно попадёт в таблицу
            // Создаём АНОНИМНЫЙ ОБЪЕКТ
            .Select(cs => new
            {
                // Строка с информацией об авто
                // Если убрать Select — DataGridView покажет ВСЮ сущность
                Auto = cs.Car.Brand + " " +
                       cs.Car.Model + " (" +
                       cs.Car.RegistrationNumber + ")",

                // Название услуги
                Service = cs.Service.Name,

                // Цена услуги
                Price = cs.Service.Price,

                // Дата обслуживания
                // ToShortDateString() — короткий формат даты
                // Без этого будет длинная дата с временем
                Date = cs.DateOfService.ToShortDateString(),

                // Пробег
                Mileage = cs.Mileage
            })

            // Выполняем запрос к БД
            // Без ToList() запрос НЕ выполнится
            .ToList();

        // Устанавливаем источник данных для DataGridView
        // Именно здесь данные появляются в таблице
        dataGridView4.DataSource = list;
    }
    catch (Exception ex)
    {
        // Если произошла ЛЮБАЯ ошибка:
        // показываем сообщение вместо падения программы
        MessageBox.Show("Viga otsimisel: " + ex.Message);
    }
}

