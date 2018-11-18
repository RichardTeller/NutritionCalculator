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
    /// Interaction logic for SearchRecipeWindow.xaml
    /// </summary>
    public partial class SearchRecipeWindow : Window
    {
        public MainWindow mainWindow = null;

        public SearchRecipeWindow()
        {
            InitializeComponent();
        }

        private void button_Search_Click(object sender, RoutedEventArgs e)
        {
            textBox_RecipeName.Text = "Recipe Name";
            textBox_RecipeIngredients.Text = "Ingredients will go here.";
            textBox_RecipeDirections.Text = "Directions will go here.";
            textBox_RecipeNotes.Text = "Notes will go here.";

            listBox_SearchResults.Items.Clear();

            String searchName = textBox_SearchRecipeByName.Text;
            System.Collections.IList selectedIngredients = listBox_SearchByIngredientResults.SelectedItems;

            if (searchName != "" && selectedIngredients.Count > 0)
            {
                // search both
                foreach (Recipe r in mainWindow.recipeDatabaseList)
                {
                    if (r.Name.ToLower().Contains(searchName.ToLower()))
                    {
                        listBox_SearchResults.Items.Add(r);
                    }
                    else
                    {
                        // search by ingredients
                        foreach (Ingredient i in selectedIngredients)
                        {
                            foreach (RecipeIngredient ri in r.ingredients)
                            {
                                if (ri.Ingredient.Equals(i))
                                {
                                    listBox_SearchResults.Items.Add(r);
                                }
                            }
                        }
                    }
                }
            }
            else if (searchName != "" && selectedIngredients.Count == 0)
            {
                // search only by recipe name
                foreach (Recipe r in mainWindow.recipeDatabaseList)
                {
                    if (r.Name.ToLower().Contains(searchName.ToLower()))
                        listBox_SearchResults.Items.Add(r);
                }
            }
            else if (searchName == "" && selectedIngredients.Count > 0)
            {
                // search only by selected ingredients
                foreach (Recipe r in mainWindow.recipeDatabaseList)
                {
                    foreach (Ingredient i in selectedIngredients)
                    {
                        foreach (RecipeIngredient ri in r.ingredients)
                        {
                            if (ri.Ingredient.Equals(i))
                            {
                                listBox_SearchResults.Items.Add(r);
                            }
                        }
                    }
                }
            }
            else if (searchName == "" && selectedIngredients.Count == 0)
            {
                // display all recipes
                foreach (Recipe r in mainWindow.recipeDatabaseList)
                {
                    listBox_SearchResults.Items.Add(r);
                }
            }
        }

        private void button_SearchIngredient_Click(object sender, RoutedEventArgs e)
        {
            listBox_SearchByIngredientResults.Items.Clear(); // clear existing results

            String search = textBox_SearchRecipeByIngredient.Text;

            if (search != "")
            {
                foreach (Ingredient i in mainWindow.ingredientDatabaseList)
                {
                    if (i.Name.ToLower().Contains(search.ToLower()))
                    {
                        listBox_SearchByIngredientResults.Items.Add(i);
                    }
                }
            }
            else // display all ingredients
            {
                foreach (Ingredient i in mainWindow.ingredientDatabaseList)
                {
                    if (i.Name.Length > 0)
                    {
                        listBox_SearchByIngredientResults.Items.Add(i);
                    }
                }
            }
        }

        private void listBox_SearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Recipe r = (Recipe)listBox_SearchResults.SelectedItem;

            if (r != null)
            {
                textBox_RecipeName.Text = r.Name;
                textBox_RecipeDirections.Text = r.Directions;
                textBox_RecipeNotes.Text = r.Notes;

                String ingredients = "";
                foreach (RecipeIngredient ri in r.ingredients)
                {
                    ingredients += " - " + ri.Quantity + " " + ri.MeasureType + " " + ri.Ingredient.ToString() + "\n";
                }
                if (ingredients.Length > 0)
                    ingredients = ingredients.Substring(0, ingredients.Length - 1);

                textBox_RecipeIngredients.Text = ingredients;

                button_LoadIntoMainWindow.IsEnabled = true;
            }
            else
            {
                textBox_RecipeName.Text = "Recipe Name";
                textBox_RecipeIngredients.Text = "Ingredients will go here.";
                textBox_RecipeDirections.Text = "Directions will go here.";
                textBox_RecipeNotes.Text = "Notes will go here.";

                button_LoadIntoMainWindow.IsEnabled = false;
            }
        }

        private void button_LoadIntoMainWindow_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.menuItem_StartNewRecipe_Click(sender, e);

            Recipe r = (Recipe)listBox_SearchResults.SelectedItem;

            for (int i = 0; i < r.ingredients.Count; i++)
            {
                Grid g = mainWindow.recipeIngredientList[i];

                UIElementCollection gridChildren = g.Children;
                TextBox ingrQty = (TextBox)gridChildren[0];
                ComboBox ingrMsr = (ComboBox)gridChildren[1];
                ComboBox ingrIngredient = (ComboBox)gridChildren[3];

                RecipeIngredient ri = r.ingredients[i];
                ingrQty.Text = ri.Quantity.ToString();
                ingrMsr.SelectedItem = ri.MeasureType;
                ingrIngredient.SelectedItem = ri.Ingredient;

                if (i < r.ingredients.Count - 1)
                    mainWindow.AddIngredientGrid();
            }

            mainWindow.button_ConfirmIngredients_Click(sender, e);

            mainWindow.textBox_RecipeName.Text = r.Name;
            mainWindow.textBox_RecipeDirections.Text = r.Directions;
            mainWindow.textBox_RecipeNotes.Text = r.Notes;

            this.Close();
        }
    }
}
