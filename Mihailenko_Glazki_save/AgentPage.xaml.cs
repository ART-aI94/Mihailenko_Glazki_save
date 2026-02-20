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

namespace Mihailenko_Glazki_save
{
    /// <summary>
    /// Логика взаимодействия для AgentPage.xaml
    /// </summary>
    public partial class AgentPage : Page
    {
        int CountRecords;
        int CountPage;
        int CurrentPage = 0;
        List<Agent> CurrentPageList = new List<Agent>();
        List<Agent> TableList;
        public AgentPage()
        {
            InitializeComponent();
            var currentAgent = MihailenkoGlazkiEntities.GetContext().Agent.ToList();
            AgentListView.ItemsSource = currentAgent;
            ComboType.SelectedIndex = 0;
            ComboType2.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAgents();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAgents();
        }


        private void ComboType2_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            UpdateAgents();
        }

        private void ChangePriorityBtn_Click(object sender, RoutedEventArgs e)
        {
            int maxPriority = 0;
            foreach (Agent selectedAgent in AgentListView.SelectedItems)
            {
                if (selectedAgent.Priority > maxPriority)
                {
                    maxPriority = selectedAgent.Priority;
                }
            }
            PriorChange prior = new PriorChange(maxPriority);
            prior.ShowDialog();

            int newPriority = Convert.ToInt32(prior.TBPriority.Text);
            foreach (Agent agent in AgentListView.SelectedItems)
            {
                agent.Priority = newPriority;
            }
        }

        private void addAgentBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AgentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AgentListView.SelectedItems.Count > 0)
            {
                ChangePriorityBtn.Visibility = Visibility.Visible;
            }
            else
            {
                ChangePriorityBtn.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateAgents()
        {
            var currentAgents = MihailenkoGlazkiEntities.GetContext().Agent.ToList();
            if (ComboType.SelectedIndex == 0) currentAgents = currentAgents.Where(p => (p.AgentTypeID >= 0 && p.AgentTypeID <= 6)).ToList();
            if (ComboType.SelectedIndex == 1) currentAgents = currentAgents.Where(p => (p.AgentTypeID == 1)).ToList();
            if (ComboType.SelectedIndex == 2) currentAgents = currentAgents.Where(p => (p.AgentTypeID == 2)).ToList();
            if (ComboType.SelectedIndex == 3) currentAgents = currentAgents.Where(p => (p.AgentTypeID == 3)).ToList();
            if (ComboType.SelectedIndex == 4) currentAgents = currentAgents.Where(p => (p.AgentTypeID == 4)).ToList();
            if (ComboType.SelectedIndex == 5) currentAgents = currentAgents.Where(p => (p.AgentTypeID == 5)).ToList();
            if (ComboType.SelectedIndex == 6) currentAgents = currentAgents.Where(p => (p.AgentTypeID == 6)).ToList();
            if (ComboType2.SelectedIndex == 0) { }
            if (ComboType2.SelectedIndex == 1) currentAgents = currentAgents.OrderBy(p => p.Title).ToList();
            if (ComboType2.SelectedIndex == 2) currentAgents = currentAgents.OrderByDescending(p => p.Title).ToList();
            if (ComboType2.SelectedIndex == 3) currentAgents = currentAgents.OrderBy(p => p.Discount).ToList();
            if (ComboType2.SelectedIndex == 4) currentAgents = currentAgents.OrderByDescending(p => p.Discount).ToList();
            if (ComboType2.SelectedIndex == 5) currentAgents = currentAgents.OrderBy(p => p.Priority).ToList();
            if (ComboType2.SelectedIndex == 6) currentAgents = currentAgents.OrderByDescending(p => p.Priority).ToList();

            currentAgents = currentAgents.Where(p =>
            p.Title.ToLower().Contains(TBoxSearch.Text.ToLower())
            || p.Phone.Replace("+7", "8").Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "")
            .Contains(TBoxSearch.Text.Replace("+7", "8").Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", ""))
            || p.Email.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();
            AgentListView.ItemsSource = currentAgents;
            TableList = currentAgents;
            ChangePage(0, 0);
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null);
        }

        private void ChangePage(int direction, int? selectedPage)
        {
            CurrentPageList.Clear();

            CountRecords = TableList.Count;

            if (CountRecords == 0)
            {
                CountPage = 0;
                return;
            }
            int recordsPerPage = 10;  
            int fullPagesCount = CountRecords / recordsPerPage;
            bool Ostatok = CountRecords % recordsPerPage > 0;

            CountPage = fullPagesCount;
            if (Ostatok)
            {
                CountPage++; 
            }

            int newPage = CurrentPage;

            if (selectedPage.HasValue)
            {
                newPage = selectedPage.Value;
            }
            else
            {
                if (direction == 1) 
                    newPage = CurrentPage - 1;
                else if (direction == 2) 
                    newPage = CurrentPage + 1;
            }

            if (newPage < 0 || newPage >= CountPage)
                return;

            CurrentPage = newPage;

            int startIndex = CurrentPage * recordsPerPage;
            int endIndex = startIndex + recordsPerPage;

            if (endIndex > CountRecords)
                endIndex = CountRecords;

            for (int i = startIndex; i < endIndex; i++)
            {
                CurrentPageList.Add(TableList[i]);
            }

            UpdatePageControls(endIndex);
        }

        private void UpdatePageControls(int endIndex)
        {
            PageListBox.Items.Clear();
            for (int i = 1; i <= CountPage; i++)
            {
                PageListBox.Items.Add(i);
            }
            PageListBox.SelectedIndex = CurrentPage;

            TBCount.Text = endIndex.ToString();
            TBAllRecords.Text = $" из {CountRecords}";

            AgentListView.ItemsSource = CurrentPageList;
            AgentListView.Items.Refresh();
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1);
        }
    }
}
