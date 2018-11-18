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

namespace NutritionCalculator
{
    /// <summary>
    /// Interaction logic for SearchEditIngredientWindow.xaml
    /// </summary>
    public partial class SearchEditIngredientWindow : Window
    {
        public Ingredient ingredient = null;
        public Ingredient selectedIngredient = null;
        public MainWindow mainWindow = null;

        public SearchEditIngredientWindow()
        {
            InitializeComponent();
        }

        private void button_ApplySearch_Click(object sender, RoutedEventArgs e)
        {
            listBox_SearchResults.Items.Clear(); // clear existing results

            String search = textBox_SearchIngredient.Text;

            if (search != "")
            {
                foreach (Ingredient i in mainWindow.ingredientDatabaseList)
                {
                    if (i.Name.ToLower().Contains(search.ToLower()))
                    {
                        listBox_SearchResults.Items.Add(i);
                    }
                }
            }
            else
            {
                foreach (Ingredient i in mainWindow.ingredientDatabaseList)
                {
                    if (i.Name.Length > 0)
                        listBox_SearchResults.Items.Add(i);
                }
            }
        }

        private void listBox_SearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Ingredient i = (Ingredient)listBox_SearchResults.SelectedItem;

            if (i != null)
            {
                selectedIngredient = i;
                button_SaveChanges.IsEnabled = true;

                textBox_Name.Text = i.Name;
                textBox_ServingQty.Text = i.ServingQty.ToString();
                comboBox_ServingMsr.Text = i.ServingMsr;
                textBox_ServingPerContainer.Text = i.ServingPerContainer.ToString();
                textBox_Calories.Text = i.Calories.ToString();
                textBox_Fat.Text = i.Fat.ToString();
                textBox_SatFat.Text = i.SatFat.ToString();
                textBox_TransFat.Text = i.TransFat.ToString();
                textBox_Cholesterol.Text = i.Cholesterol.ToString();
                textBox_Sodium.Text = i.Sodium.ToString();
                textBox_Carbs.Text = i.Carbs.ToString();
                textBox_Fiber.Text = i.Fiber.ToString();
                textBox_Sugar.Text = i.Sugar.ToString();
                textBox_Protein.Text = i.Protein.ToString();
                textBox_Price.Text = i.Price.ToString();
            }
            else
            {
                button_SaveChanges.IsEnabled = false;

                textBox_Name.Text = "";
                textBox_ServingQty.Text = "";
                comboBox_ServingMsr.Text = "";
                textBox_ServingPerContainer.Text = "";
                textBox_Calories.Text = "";
                textBox_Fat.Text = "";
                textBox_SatFat.Text = "";
                textBox_TransFat.Text = "";
                textBox_Cholesterol.Text = "";
                textBox_Sodium.Text = "";
                textBox_Carbs.Text = "";
                textBox_Fiber.Text = "";
                textBox_Sugar.Text = "";
                textBox_Protein.Text = "";
                textBox_Price.Text = "";
            }
        }

        private void button_SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            bool emptyField = false, invalidField = false;

            String Name = "", ServingMsr = "";
            double ServingQty = 0, ServingPerContainer = 0, Calories = 0, Fat = 0, SatFat = 0, TransFat = 0, Cholesterol = 0,
                Sodium = 0, Carbs = 0, Fiber = 0, Sugar = 0, Protein = 0, Price = 0;

            if (textBox_Name.Text != "")
            {
                Name = textBox_Name.Text;
            }
            else
                emptyField = true;

            if (textBox_ServingQty.Text != "")
            {
                try
                {
                    ServingQty = Double.Parse(textBox_ServingQty.Text);
                    if (ServingQty <= 0)
                        invalidField = true;
                }
                catch (Exception)
                {
                    invalidField = true;
                }
            }
            else
                emptyField = true;

            if (comboBox_ServingMsr.Text != "")
            {
                ServingMsr = comboBox_ServingMsr.Text;
            }
            else
                emptyField = true;

            if (textBox_ServingPerContainer.Text != "")
            {
                try
                {
                    ServingPerContainer = Double.Parse(textBox_ServingPerContainer.Text);
                    if (ServingPerContainer <= 0)
                        invalidField = true;
                }
                catch (Exception)
                {
                    invalidField = true;
                }

            }
            else
                emptyField = true;

            if (textBox_Calories.Text != "")
            {
                try
                {
                    Calories = Double.Parse(textBox_Calories.Text);
                    if (Calories < 0)
                        invalidField = true;
                }
                catch (Exception)
                {
                    invalidField = true;
                }
            }
            else
                emptyField = true;

            if (textBox_Fat.Text != "")
            {
                try
                {
                    Fat = Double.Parse(textBox_Fat.Text);
                    if (Fat < 0)
                        invalidField = true;
                }
                catch (Exception)
                {
                    invalidField = true;
                }
            }
            else
                emptyField = true;

            if (textBox_SatFat.Text != "")
            {
                try
                {
                    SatFat = Double.Parse(textBox_SatFat.Text);
                    if (SatFat < 0)
                        invalidField = true;
                }
                catch (Exception)
                {
                    invalidField = true;
                }
            }
            else
                emptyField = true;

            if (textBox_TransFat.Text != "")
            {
                try
                {
                    TransFat = Double.Parse(textBox_TransFat.Text);
                    if (TransFat < 0)
                        invalidField = true;
                }
                catch (Exception)
                {
                    invalidField = true;
                }
            }
            else
                emptyField = true;

            if (textBox_Cholesterol.Text != "")
            {
                try
                {
                    Cholesterol = Double.Parse(textBox_Cholesterol.Text);
                    if (Cholesterol < 0)
                        invalidField = true;
                }
                catch (Exception)
                {
                    invalidField = true;
                }
            }
            else
                emptyField = true;

            if (textBox_Sodium.Text != "")
            {
                try
                {
                    Sodium = Double.Parse(textBox_Sodium.Text);
                    if (Sodium < 0)
                        invalidField = true;
                }
                catch (Exception)
                {
                    invalidField = true;
                }
            }
            else
                emptyField = true;

            if (textBox_Carbs.Text != "")
            {
                try
                {
                    Carbs = Double.Parse(textBox_Carbs.Text);
                    if (Carbs < 0)
                        invalidField = true;
                }
                catch (Exception)
                {
                    invalidField = true;
                }
            }
            else
                emptyField = true;

            if (textBox_Fiber.Text != "")
            {
                try
                {
                    Fiber = Double.Parse(textBox_Fiber.Text);
                    if (Fiber < 0)
                        invalidField = true;
                }
                catch (Exception)
                {
                    invalidField = true;
                }
            }
            else
                emptyField = true;

            if (textBox_Sugar.Text != "")
            {
                try
                {
                    Sugar = Double.Parse(textBox_Sugar.Text);
                    if (Sugar < 0)
                        invalidField = true;
                }
                catch (Exception)
                {
                    invalidField = true;
                }
            }
            else
                emptyField = true;

            if (textBox_Protein.Text != "")
            {
                try
                {
                    Protein = Double.Parse(textBox_Protein.Text);
                    if (Protein < 0)
                        invalidField = true;
                }
                catch (Exception)
                {
                    invalidField = true;
                }
            }
            else
                emptyField = true;

            if (textBox_Price.Text != "")
            {
                try
                {
                    Price = Double.Parse(textBox_Price.Text);
                    if (Price < 0)
                        invalidField = true;
                }
                catch (Exception)
                {
                    invalidField = true;
                }
            }
            else
                emptyField = true;

            if (emptyField == true)
            {
                MessageBox.Show("At least one of the fields was left empty!\nPlease ensure all fields are occupied.");
                ingredient = null;
            }
            else if (invalidField == true)
            {
                MessageBox.Show("Alpha input or a negative number was detected!\nPlease enter only positive numbers.");
                ingredient = null;
            }
            else
            {
                ingredient = new Ingredient(Name, ServingQty, ServingMsr, ServingPerContainer, Calories, Fat, SatFat, TransFat,
                    Cholesterol, Sodium, Carbs, Fiber, Sugar, Protein, Price);

                mainWindow.ingredientDatabaseList.Remove(selectedIngredient);
                mainWindow.ingredientDatabaseList.Add(ingredient);
                mainWindow.ingredientDatabaseList.Sort();

                button_ApplySearch_Click(sender, e);
            }
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
