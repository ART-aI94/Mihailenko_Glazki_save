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
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Data.Entity;

namespace Mihailenko_Glazki_save
{
    /// <summary>
    /// Логика взаимодействия для SalesHistoryWindow.xaml
    /// </summary>
    public partial class SalesHistoryWindow : Window
    {
        private int currentAgentId;
        private ProductSale selectedSale;

        public SalesHistoryWindow(int agentId)
        {
            InitializeComponent();
            currentAgentId = agentId;
            LoadData();
        }

        private void LoadData()
        {
            ProductBox.ItemsSource = MihailenkoGlazkiEntities.GetContext().Product.ToList();

            UpdateSalesList();
        }

        private void UpdateSalesList()
        {
            var sales = MihailenkoGlazkiEntities.GetContext().ProductSale
                .Where(ps => ps.AgentID == currentAgentId)
                .Include(ps => ps.Product)
                .OrderByDescending(ps => ps.SaleDate)
                .ToList();

            SalesList.ItemsSource = sales;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (ProductBox.SelectedItem == null)
            {
                errors.AppendLine("Выберите продукт");
            }

            if (string.IsNullOrWhiteSpace(CountBox.Text))
            {
                errors.AppendLine("Укажите количество");
            }
            else
            {
                if (!int.TryParse(CountBox.Text, out int count) || count <= 0)
                {
                    errors.AppendLine("Количество должно быть положительным числом");
                }
            }

            if (DateBox.SelectedDate == null)
            {
                errors.AppendLine("Укажите дату продажи");
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка", MessageBoxButton.OK);
                return;
            }

            try
            {
                ProductSale newSale = new ProductSale
                {
                    AgentID = currentAgentId,
                    ProductID = (ProductBox.SelectedItem as Product).ID,
                    ProductCount = int.Parse(CountBox.Text),
                    SaleDate = DateBox.SelectedDate.Value
                };

                MihailenkoGlazkiEntities.GetContext().ProductSale.Add(newSale);
                MihailenkoGlazkiEntities.GetContext().SaveChanges();

                ProductBox.SelectedItem = null;
                CountBox.Text = "1";
                DateBox.SelectedDate = DateTime.Today;

                UpdateSalesList();

                MessageBox.Show("Продажа добавлена", "Успешно", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSale == null)
                return;

            if (MessageBox.Show("Удалить запись о продаже?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    MihailenkoGlazkiEntities.GetContext().ProductSale.Remove(selectedSale);
                    MihailenkoGlazkiEntities.GetContext().SaveChanges();

                    selectedSale = null;
                    DeleteBtn.IsEnabled = false;

                    UpdateSalesList();
                    MessageBox.Show("Запись удалена", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SalesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedSale = SalesList.SelectedItem as ProductSale;
            DeleteBtn.IsEnabled = selectedSale != null;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void ProductBox_KeyUp(object sender, KeyEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            string searchText = comboBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                comboBox.ItemsSource = MihailenkoGlazkiEntities.GetContext().Product.ToList();
            }
            else
            {
                var filteredProducts = MihailenkoGlazkiEntities.GetContext().Product
                    .Where(p => p.Title.ToLower().Contains(searchText))
                    .ToList();

                comboBox.ItemsSource = filteredProducts;
                comboBox.IsDropDownOpen = true;
            }
        }
    }
}