using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity;

namespace Mihailenko_Glazki_save
{
    /// <summary>
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        private Agent currentAgent = new Agent();
        private int oldPriority;
        public AddEditPage(Agent selectedAgent)
        {
            InitializeComponent();
            ComboType.ItemsSource = MihailenkoGlazkiEntities.GetContext().AgentType.ToList();

            if (selectedAgent != null)
            {
                currentAgent = selectedAgent;
                oldPriority = currentAgent.Priority;
                ComboType.SelectedValue = currentAgent.AgentTypeID;
            }
            DataContext = currentAgent;
        }

        private void ChangePictureBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog myOpenFileDialog = new OpenFileDialog();
            if (myOpenFileDialog.ShowDialog() == true)
            {
                string fileName = System.IO.Path.GetFileName(myOpenFileDialog.FileName);
                currentAgent.Logo = "agents/" + fileName;
                LogoImage.Source = new BitmapImage(new Uri(myOpenFileDialog.FileName));
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(currentAgent.Title))
                errors.AppendLine("Укажите наименование агента");
            if (string.IsNullOrWhiteSpace(currentAgent.Address))
                errors.AppendLine("Укажите адрес агента");
            if (string.IsNullOrWhiteSpace(currentAgent.DirectorName))
                errors.AppendLine("Укажите Фио диретора");
            if (ComboType.SelectedItem == null)
                errors.AppendLine("Укажите тип агента");
            if (string.IsNullOrWhiteSpace(currentAgent.Priority.ToString()))
                errors.AppendLine("Укажите приоритет");
            if (currentAgent.Priority < 0)
                errors.AppendLine("Укажите положительный приоритет агента");
            if (string.IsNullOrWhiteSpace(currentAgent.INN))
                errors.AppendLine("Укажите ИНН агента");
            if (string.IsNullOrWhiteSpace(currentAgent.KPP))
                errors.AppendLine("Укажите КПП агента");
            if (string.IsNullOrWhiteSpace(currentAgent.Phone))
                errors.AppendLine("Укажите телефон агента");
            if (string.IsNullOrWhiteSpace(currentAgent.Email))
                errors.AppendLine("Укажите почту агента");
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (ComboType.SelectedItem != null)
            {
                var selectedType = ComboType.SelectedItem as AgentType;
                if (selectedType != null)
                {
                    currentAgent.AgentTypeID = selectedType.ID;
                }
            }

            var context = MihailenkoGlazkiEntities.GetContext();
            if (currentAgent.ID != 0 && oldPriority != currentAgent.Priority)
            {
                AgentPriorityHistory history = new AgentPriorityHistory
                {
                    AgentID = currentAgent.ID,
                    ChangeDate = DateTime.Now,
                    PriorityValue = currentAgent.Priority 
                };

                context.AgentPriorityHistory.Add(history);

                oldPriority = currentAgent.Priority;
            }

            if (currentAgent.ID == 0)
                MihailenkoGlazkiEntities.GetContext().Agent.Add(currentAgent);
            try
            {
                MihailenkoGlazkiEntities.GetContext().SaveChanges();
                MessageBox.Show("Информация сохранена");
                Manager.MainFrame.Navigate(new AgentPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentAgent.ID == 0)
            {
                MessageBox.Show("Нельзя удалить нового агента до сохранения");
                return;
            }

            var context = MihailenkoGlazkiEntities.GetContext();

            int salesCount = context.ProductSale.Count(ps => ps.AgentID == currentAgent.ID);

            if (salesCount > 0)
            {
                MessageBox.Show($"Нельзя удалить агента с продажами (найдено {salesCount} записей)");
                return;
            }

            if (MessageBox.Show("Удалить агента?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var history = context.AgentPriorityHistory.Where(h => h.AgentID == currentAgent.ID);
                foreach (var h in history)
                    context.AgentPriorityHistory.Remove(h);

                var shops = context.Shop.Where(s => s.AgentID == currentAgent.ID);
                foreach (var s in shops)
                    context.Shop.Remove(s);

                var agent = context.Agent.Find(currentAgent.ID);
                context.Agent.Remove(agent);

                context.SaveChanges();
                MessageBox.Show("Агент удален");
                Manager.MainFrame.Navigate(new AgentPage());
            }
        }

        private void HistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentAgent.ID == 0)
            {
                MessageBox.Show("Сначала сохраните агента");
                return;
            }
            var win = new SalesHistoryWindow(currentAgent.ID);
            win.ShowDialog();
        }
    }
}
