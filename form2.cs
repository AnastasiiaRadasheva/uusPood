
namespace Autod
{
    public partial class Form2 : Form // Объявляем класс формы Form2, наследующий Form
    {
        private readonly AutoDbContext _db; // Приватное поле для работы с базой данных через Entity Framework
        // readonly означает, что поле можно задать только в конструкторе и не менять после

        public Form2()
        {
            InitializeComponent(); // Инициализация компонентов формы. Без этого форма не будет корректно отображаться.
        }

        // Конструктор с передачей контекста базы данных
        public Form2(AutoDbContext db) : this() // Вызывает конструктор по умолчанию
        {
            _db = db; // Сохраняем ссылку на контекст базы данных
            PopulateCheckedListBox(); 
            // Вызываем функцию для заполнения CheckedListBox машинами из базы.
            // Без этой функции список машин будет пустым, пользователь не сможет ничего выбрать
        }

        // Вспомогательный класс для представления машины в CheckedListBox
        private class CarListItem
        {
            public int Id { get; set; } // ID машины из базы
            public string DisplayText { get; set; } // Текст, который будет отображаться в списке
        }

        // Функция для заполнения CheckedListBox машинами из базы
        private void PopulateCheckedListBox()
        {
            // Берём все машины из базы и создаём для каждой объект CarListItem
            var cars = _db.Cars
                .Select(c => new CarListItem
                {
                    Id = c.Id, // сохраняем ID машины
                    DisplayText = $"{c.Brand}/{c.RegistrationNumber}" // отображаем бренд и регистрационный номер
                })
                .ToList(); // формируем список для привязки к контролу

            // Привязываем список к CheckedListBox
            checkedListBoxAutod.DataSource = cars; // источник данных
            checkedListBoxAutod.DisplayMember = nameof(CarListItem.DisplayText); // показываем DisplayText пользователю
            checkedListBoxAutod.ValueMember = nameof(CarListItem.Id); // используем Id в коде при необходимости
            // Без этой функции CheckedListBox будет пустым
        }

        // Функция для получения выбранных машин пользователем
        public List<string> GetSelectedCars()
        {
            var selectedCars = new List<string>(); // создаём список для хранения выбранных машин

            foreach (var selectedItem in checkedListBoxAutod.CheckedItems) // проходим по всем выбранным элементам
            {
                var car = selectedItem as dynamic; // приводим к динамическому типу для доступа к DisplayText
                if (car != null) // проверяем, что объект не null
                {
                    selectedCars.Add(car.DisplayText); // добавляем текст отображения в список выбранных машин
                }
            }

            return selectedCars; // возвращаем список выбранных машин
            // Без этой функции невозможно узнать, что выбрал пользователь
        }

        // Событие нажатия кнопки OK
        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK; // устанавливаем результат формы как OK
            this.Close(); // закрываем форму
            // Без этой функции форма не будет закрываться корректно при нажатии на кнопку
        }

        // Функция для применения локализации ко всем контролам
        private void ApplyResourcesToControl(Control ctrl, ComponentResourceManager res)
        {
            res.ApplyResources(ctrl, ctrl.Name); // применяем ресурсы к текущему контролу
            foreach (Control child in ctrl.Controls) // рекурсивно проходим по всем дочерним контролам
            {
                ApplyResourcesToControl(child, res); // применяем ресурсы ко всем дочерним элементам
            }
            // Без этой функции форма не будет корректно менять язык интерфейса
        }

        // Событие загрузки формы
        private void Form2_Load(object sender, EventArgs e)
        {
            // Пустой обработчик, можно добавить логику, которая должна выполняться при загрузке формы
        }
    }
}
