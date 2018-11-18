/* Author: Richard Teller
 * 
 * Description of program finer details: (program usability can be found by selecting user manual from the help menu when the program is running)
 * 
 * When the program starts it will load in from the ingredient and recipe databases.  It will load the databases into the list objects ingredientDatabaseList and recipeDatabaseList.
 * Once these lists are created the database is not used until the program exits, at which point the ingredient and recipe lists are written to the databases, the database tables 
 * are completely deleted and recreated and are filled with the current ingredient and recipe lists.
 * 
 * The ingredient list for your recipe on the left is managed by a List<Grid> object and is named recipeIngredientList.
 * 
 */

using System;
using System.IO;
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
using System.Data.SQLite;
using System.Data;
using Microsoft.Win32;

namespace NutritionCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SQLiteConnection SQL_connection;
        private SQLiteCommand SQL_cmd;
        private SQLiteDataReader SQL_datareader;

        public List<Ingredient> ingredientDatabaseList;
        public List<Grid> recipeIngredientList;

        public List<Recipe> recipeDatabaseList;

        public Ingredient newIngredient = null;
        private int n_currentIngredients = 0;
        private Recipe currentRecipe;

        private bool displayingNutrition;
        private bool displayingPerServing;


        public MainWindow()
        {
            InitializeComponent();

            SetupSQLDatabase();
            ingredientDatabaseList = BuildIngredientDatabaseList();
            BuildRecipeIngredientList();
            recipeDatabaseList = BuildRecipeDatabaseList();
            currentRecipe = new Recipe();

            RefreshIngredientDropdowns();
            displayingNutrition = true;
            displayingPerServing = true;
        }

        #region Startup
        private void SetupSQLDatabase()
        {
            //create a new database connection:
            SQL_connection = new SQLiteConnection("Data Source=database.db;Version=3;New=True;Compress=True;");
            SQL_connection.Open(); // open the connection

            //create a new SQL command:
            SQL_cmd = SQL_connection.CreateCommand();

            //create the tables
            SQL_cmd.CommandText = "CREATE TABLE IF NOT EXISTS Ingredients (Name TEXT, ServingQty REAL, ServingMsr TEXT, ServingPerContainer REAL, Calories REAL, Fat REAL, SatFat REAL, TransFat REAL, Cholesterol REAL, Sodium REAL, Carbs REAL, Fiber REAL, Sugar REAL, Protein REAL, Price REAL);";
            SQL_cmd.ExecuteNonQuery();

            SQL_cmd.CommandText = "CREATE TABLE IF NOT EXISTS Recipes (Name TEXT, Ingredients TEXT, Directions TEXT, Notes TEXT);";
            SQL_cmd.ExecuteNonQuery();
        }

        private List<Ingredient> BuildIngredientDatabaseList()
        {
            List<Ingredient> list = new List<Ingredient>();
            list.Add(new Ingredient()); // give blank ingredient option

            String Name = "", ServingMsr = "";
            double ServingQty = 0, ServingPerContainer = 0, Calories = 0, Fat = 0, SatFat = 0, TransFat = 0, Cholesterol = 0,
                Sodium = 0, Carbs = 0, Fiber = 0, Sugar = 0, Protein = 0, Price = 0;

            // read from table, create Ingredient object, add to list
            SQL_cmd.CommandText = "SELECT * FROM Ingredients;";
            SQL_datareader = SQL_cmd.ExecuteReader();

            while (SQL_datareader.Read())
            {
                // there is a table entry to read
                Name = (String)SQL_datareader["Name"];
                ServingQty = (double)SQL_datareader["ServingQty"];
                ServingMsr = (String)SQL_datareader["ServingMsr"];
                ServingPerContainer = (double)SQL_datareader["ServingPerContainer"];
                Calories = (double)SQL_datareader["Calories"];
                Fat = (double)SQL_datareader["Fat"];
                SatFat = (double)SQL_datareader["SatFat"];
                TransFat = (double)SQL_datareader["TransFat"];
                Cholesterol = (double)SQL_datareader["Cholesterol"];
                Sodium = (double)SQL_datareader["Sodium"];
                Carbs = (double)SQL_datareader["Carbs"];
                Fiber = (double)SQL_datareader["Fiber"];
                Sugar = (double)SQL_datareader["Sugar"];
                Protein = (double)SQL_datareader["Protein"];
                Price = (double)SQL_datareader["Price"];

                // add ingredient to list
                Ingredient ingredient = new Ingredient(Name, ServingQty, ServingMsr, ServingPerContainer, Calories, Fat, SatFat, TransFat,
                    Cholesterol, Sodium, Carbs, Fiber, Sugar, Protein, Price);

                list.Add(ingredient);
            }

            SQL_datareader.Close();

            return list;
        }

        private void BuildRecipeIngredientList()
        {
            recipeIngredientList = new List<Grid>();

            AddIngredientGrid();
        }

        private List<Recipe> BuildRecipeDatabaseList()
        {
            bool abortRead = false;
            List<Recipe> list = new List<Recipe>();

            String name = "", ingredients = "", directions = "", notes = "";

            // read from table, create Recipe object, add to list
            SQL_cmd.CommandText = "SELECT * FROM Recipes;";
            SQL_datareader = SQL_cmd.ExecuteReader();

            while (SQL_datareader.Read())
            {
                // there is a table entry to read
                name = (String)SQL_datareader["Name"];
                ingredients = (String)SQL_datareader["Ingredients"];
                directions = (String)SQL_datareader["Directions"];
                notes = (String)SQL_datareader["Notes"];

                Recipe recipe = new Recipe();
                recipe.Name = name;
                recipe.Directions = directions;
                recipe.Notes = notes;

                // add the ingredients to the recipe object
                String[] subStrings = ingredients.Split('~');
                if (subStrings.Length % 3 == 0) // each ingredient will have quantity, measure type and ingredient, so each ingredient should have 3 strings associated with it
                {
                    for (int i = 0; i < subStrings.Length; i++)
                    {
                        if (!abortRead)
                        {
                            RecipeIngredient rIngredient = new RecipeIngredient();

                            // read quantity
                            if (!Double.TryParse(subStrings[i], out rIngredient.Quantity)) // if failed
                                abortRead = true;

                            i++;

                            // read measure type
                            rIngredient.MeasureType = subStrings[i];
                            i++;

                            // find the ingredient object
                            bool ingredientFound = false;
                            foreach (Ingredient dbIngredient in ingredientDatabaseList)
                            {
                                if (dbIngredient.Name == subStrings[i])
                                {
                                    rIngredient.Ingredient = dbIngredient;
                                    ingredientFound = true;
                                }
                            }

                            if (!ingredientFound)
                            {
                                abortRead = true;
                                MessageBox.Show("A Recipe in the database calls for an ingredient not currently saved in the archive.");
                            }

                            recipe.ingredients.Add(rIngredient);
                        }
                    }
                }
                else
                {
                    abortRead = true;
                }

                list.Add(recipe);
            }

            SQL_datareader.Close();

            if (abortRead)
            {
                list = new List<Recipe>();
                MessageBox.Show("Error when reading from Recipe Database.\nLoading with a blank recipe list...");
            }

            return list;
        }
        #endregion

        #region Closing
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WriteIngredientsToDatabase();
            WriteRecipesToDatabase();
            SQL_connection.Close();
        }

        private void WriteIngredientsToDatabase()
        {
            if (ingredientDatabaseList.Count > 0)
                ingredientDatabaseList.RemoveAt(0); // remove the blank ingredient

            // recreate the table
            SQL_cmd.CommandText = "DROP TABLE IF EXISTS Ingredients";
            SQL_cmd.ExecuteNonQuery();

            SQL_cmd.CommandText = "CREATE TABLE IF NOT EXISTS Ingredients (Name TEXT, ServingQty REAL, ServingMsr TEXT, ServingPerContainer REAL, Calories REAL, Fat REAL, SatFat REAL, TransFat REAL, Cholesterol REAL, Sodium REAL, Carbs REAL, Fiber REAL, Sugar REAL, Protein REAL, Price REAL);";
            SQL_cmd.ExecuteNonQuery();

            // add the ingredients to database
            foreach (Ingredient i in ingredientDatabaseList)
            {
                SQL_cmd.CommandText = "INSERT INTO Ingredients (Name, ServingQty, ServingMsr, ServingPerContainer, Calories, Fat, SatFat, TransFat, Cholesterol, Sodium, Carbs, Fiber, Sugar, Protein, Price) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);";
                SQL_cmd.Parameters.Add("@Name", DbType.String).Value = i.Name;
                SQL_cmd.Parameters.Add("@ServingQty", DbType.Double).Value = i.ServingQty;
                SQL_cmd.Parameters.Add("@ServingMsr", DbType.String).Value = i.ServingMsr;
                SQL_cmd.Parameters.Add("@ServingPerContainer", DbType.Double).Value = i.ServingPerContainer;
                SQL_cmd.Parameters.Add("@Calories", DbType.Double).Value = i.Calories;
                SQL_cmd.Parameters.Add("@Fat", DbType.Double).Value = i.Fat;
                SQL_cmd.Parameters.Add("@SatFat", DbType.Double).Value = i.SatFat;
                SQL_cmd.Parameters.Add("@TransFat", DbType.Double).Value = i.TransFat;
                SQL_cmd.Parameters.Add("@Cholesterol", DbType.Double).Value = i.Cholesterol;
                SQL_cmd.Parameters.Add("@Sodium", DbType.Double).Value = i.Sodium;
                SQL_cmd.Parameters.Add("@Carbs", DbType.Double).Value = i.Carbs;
                SQL_cmd.Parameters.Add("@Fiber", DbType.Double).Value = i.Fiber;
                SQL_cmd.Parameters.Add("@Sugar", DbType.Double).Value = i.Sugar;
                SQL_cmd.Parameters.Add("@Protein", DbType.Double).Value = i.Protein;
                SQL_cmd.Parameters.Add("@Price", DbType.Double).Value = i.Price;
                SQL_cmd.ExecuteNonQuery();
            }
        }

        private void WriteRecipesToDatabase()
        {
            // recreate the table
            SQL_cmd.CommandText = "DROP TABLE IF EXISTS Recipes";
            SQL_cmd.ExecuteNonQuery();

            SQL_cmd.CommandText = "CREATE TABLE IF NOT EXISTS Recipes (Name TEXT, Ingredients TEXT, Directions TEXT, Notes TEXT);";
            SQL_cmd.ExecuteNonQuery();

            // add the recipes to database
            foreach (Recipe r in recipeDatabaseList)
            {
                String ingredients_AsString = "";

                foreach (RecipeIngredient rIngredient in r.ingredients)
                {
                    ingredients_AsString += rIngredient.Quantity.ToString() + "~";
                    ingredients_AsString += rIngredient.MeasureType + "~";
                    ingredients_AsString += rIngredient.Ingredient.ToString() + "~";
                }
                if (ingredients_AsString.Length > 0)
                    ingredients_AsString = ingredients_AsString.Substring(0, ingredients_AsString.Length - 1); // remove last tilda

                String text = "'" + r.Name + "', '" + ingredients_AsString + "', '" + r.Directions + "', '" + r.Notes + "'";
                SQL_cmd.CommandText = "INSERT INTO Recipes VALUES (" + text + ");";
                SQL_cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region MenuItems

        #region File
        public void menuItem_StartNewRecipe_Click(object sender, RoutedEventArgs e)
        {
            currentRecipe = new Recipe();
            recipeIngredientList.Clear();
            stackPanel_Ingredients.Children.Clear();
            n_currentIngredients = 0;
            AddIngredientGrid();

            // reinitalize all labels and textboxes
            label_ServingCalories.Content = "-";
            label_ServingFat.Content = "-";
            label_ServingSatFat.Content = "-";
            label_ServingTransFat.Content = "-";
            label_ServingCholesterol.Content = "-";
            label_ServingSodium.Content = "-";
            label_ServingCarbs.Content = "-";
            label_ServingFiber.Content = "-";
            label_ServingSugar.Content = "-";
            label_ServingProtein.Content = "-";
            label_ServingPrice.Content = "-";

            label_RecipeCalories.Content = "-";
            label_RecipeFat.Content = "-";
            label_RecipeSatFat.Content = "-";
            label_RecipeTransFat.Content = "-";
            label_RecipeCholesterol.Content = "-";
            label_RecipeSodium.Content = "-";
            label_RecipeCarbs.Content = "-";
            label_RecipeFiber.Content = "-";
            label_RecipeSugar.Content = "-";
            label_RecipeProtein.Content = "-";
            label_RecipePrice.Content = "-";

            textBox_RecipeName.Text = "Add a Recipe Name (Click me)";
            textBox_RecipeIngredients.Text = "Ingredients will be added here";
            textBox_RecipeDirections.Text = "Add directions here.";
            textBox_RecipeNotes.Text = "Add notes here.";

            textBox_NumberServingsMade.Text = "";
        }

        private void menuItem_SaveRecipeToText_Click(object sender, RoutedEventArgs e)
        {
            currentRecipe.Name = textBox_RecipeName.Text;
            currentRecipe.Directions = textBox_RecipeDirections.Text;
            currentRecipe.Notes = textBox_RecipeNotes.Text;

            SaveFileDialog saveWindow = new SaveFileDialog();
            saveWindow.Filter = "Text Document (*.txt)|*.txt";
            saveWindow.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            try
            {
                if (saveWindow.ShowDialog() == true)
                {
                    // save the entire recipe into the single string 'text'
                    String text = currentRecipe.Name + "\n\n";
                    text += "Ingredients\n";
                    foreach (RecipeIngredient rIngredient in currentRecipe.ingredients)
                    {
                        text += " - " + rIngredient.Quantity + " " + rIngredient.MeasureType + " " + rIngredient.Ingredient.ToString() + "\n";
                    }

                    text += "\nDirections\n";
                    text += currentRecipe.Directions + "\n\n";

                    text += "Notes\n";
                    text += currentRecipe.Notes;

                    File.WriteAllText(saveWindow.FileName, text); // help for this method came from the SaveFileDialog msdn 
                }
            }
            catch
            {
                MessageBox.Show("Error saving to text file.");
            }
        }

        private void menuItem_LoadFromCSV_Click(object sender, RoutedEventArgs e)
        {
            StreamReader stream = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "CSV files (*.csv)|*.csv";
            openFileDialog.RestoreDirectory = true;

            Nullable<bool> result = openFileDialog.ShowDialog();

            if (result == true)
            {
                String fileName = openFileDialog.FileName;

                List<Ingredient> list = new List<Ingredient>();
                list.Add(new Ingredient());

                try
                {
                    stream = new StreamReader(fileName);

                    int currentLine = 1; // use non-zero based indexing when reporting errors

                    String text = "";
                    while (stream.Peek() > -1) // while there is another line in the csv file
                    {
                        text = stream.ReadLine();

                        String[] subStrings = text.Split(',');

                        // there are 15 total columns for each ingredient
                        if (subStrings.Length == 15)
                        {
                            String Name = "", ServingMsr = "";
                            double ServingQty = 0, ServingPerContainer = 0, Calories = 0, Fat = 0, SatFat = 0, TransFat = 0, Cholesterol = 0,
                                Sodium = 0, Carbs = 0, Fiber = 0, Sugar = 0, Protein = 0, Price = 0;

                            Name = subStrings[0];
                            ServingQty = Double.Parse(subStrings[1]);
                            ServingMsr = subStrings[2];
                            ServingPerContainer = Double.Parse(subStrings[3]);
                            Calories = Double.Parse(subStrings[4]);
                            Fat = Double.Parse(subStrings[5]);
                            SatFat = Double.Parse(subStrings[6]);
                            TransFat = Double.Parse(subStrings[7]);
                            Cholesterol = Double.Parse(subStrings[8]);
                            Sodium = Double.Parse(subStrings[9]);
                            Carbs = Double.Parse(subStrings[10]);
                            Fiber = Double.Parse(subStrings[11]);
                            Sugar = Double.Parse(subStrings[12]);
                            Protein = Double.Parse(subStrings[13]);
                            Price = Double.Parse(subStrings[14]);

                            if (Name.Length == 0 || ServingQty < 0 || (ServingMsr != "tsp." && ServingMsr != "tbsp." && ServingMsr != "fl oz" && ServingMsr != "cup" && ServingMsr != "pint" && ServingMsr != "quart" && ServingMsr != "gallon" && ServingMsr != "ml" && ServingMsr != "liter" && ServingMsr != "oz" && ServingMsr != "mg" && ServingMsr != "g" && ServingMsr != "kg") || ServingPerContainer < 0 || Calories < 0 || Fat < 0 || SatFat < 0 ||
                                TransFat < 0 || Cholesterol < 0 || Sodium < 0 || Carbs < 0 || Fiber < 0 || Sugar < 0 || Protein < 0 || Price < 0)
                            {
                                MessageBox.Show("Either a field was empty, Serving Measurement Type was invalid, or a field contained a negative number");
                                throw new Exception();
                            }

                            Ingredient i = new Ingredient(Name, ServingQty, ServingMsr, ServingPerContainer, Calories, Fat, SatFat,
                                TransFat, Cholesterol, Sodium, Carbs, Fiber, Sugar, Protein, Price);

                            list.Add(i);
                        }
                        else
                        {
                            MessageBox.Show("The ingredient on line " + currentLine + " does not contain 15 values.");
                            throw new Exception();
                        }
                        currentLine++;
                    }

                    stream.Close();

                    // make ingredientDatabaseList point to the newly loaded in list
                    ingredientDatabaseList = list;
                    RefreshIngredientDropdowns();
                }
                catch
                {
                    MessageBox.Show("Error in loading from CSV.  CSV file is formatted improperly.");
                }
            }
        }

        private void menuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Ingredients/Recipes

        #region Ingredients
        private void menuItem_AddIngredient_Click(object sender, RoutedEventArgs e)
        {
            AddIngredientWindow addWindow = new AddIngredientWindow();
            addWindow.mainWindow = this;
            addWindow.ShowDialog();

            RefreshIngredientDropdowns();
        }

        private void menuItem_SearchEditIngredients_Click(object sender, RoutedEventArgs e)
        {
            SearchEditIngredientWindow searchWindow = new SearchEditIngredientWindow();
            searchWindow.mainWindow = this;
            searchWindow.ShowDialog();

            RefreshIngredientDropdowns();
        }

        private void menuItem_DisplayAllIngredients_Click(object sender, RoutedEventArgs e)
        {
            String msg = "";
            foreach (Ingredient i in ingredientDatabaseList)
            {
                msg += i.ToString() + "\n";
            }

            MessageBox.Show(msg);
        }

        private void menuItem_ClearIngredientDatabase_Click(object sender, RoutedEventArgs e)
        {
            ingredientDatabaseList.Clear();
            ingredientDatabaseList.Add(new Ingredient());
            RefreshIngredientDropdowns();
        }
        #endregion

        #region Recipes
        private void menuItem_AddRecipeToArchive_Click(object sender, RoutedEventArgs e)
        {
            if (currentRecipe.ingredients.Count > 0) // ingredients get added to currentRecipe when user clicks confirm ingredients
            {
                currentRecipe.Name = textBox_RecipeName.Text;
                currentRecipe.Directions = textBox_RecipeDirections.Text;
                currentRecipe.Notes = textBox_RecipeNotes.Text;

                recipeDatabaseList.Add(currentRecipe);
                recipeDatabaseList.Sort();
            }
            else
            {
                MessageBox.Show("Current recipe does not contain any ingredients...\nMake sure to click Confirm Ingredients before trying to add the recipe to archive.");
            }
        }

        private void menuItem_SearchRecipes_Click(object sender, RoutedEventArgs e)
        {
            SearchRecipeWindow searchWindow = new SearchRecipeWindow();
            searchWindow.mainWindow = this;
            searchWindow.ShowDialog();
        }

        private void menuItem_ClearRecipeDatabase_Click(object sender, RoutedEventArgs e)
        {
            recipeDatabaseList.Clear();
        }
        #endregion
        #endregion

        #region Help

        private void menuItem_UserManual_Click(object sender, RoutedEventArgs e)
        {
            String msg = "";

            msg = "To start a recipe, enter in the ingredients you want on the left.  In order to enter in an ingredient, it must first exist in the ingredient archive.  If it does not already exist, select Add Ingredient to Archive from the menu to add it.  If you need to add more ingredients to your recipe, click the Add Another Ingredient button.  Once you've added all of your ingredients, you may specify how many servings the recipe makes, and then click Confirm Ingredients to calculate the nutrition facts for your recipe.  You can view the nutrition facts for the recipe and for an individual serving on the right, and you can also view the recipe as well.  In the recipe view on the right you can give the recipe a name, add directions, or notes.  When you are done editing your recipe you can select Add Recipe to Archive from the menu to save the recipe for later (it can be retrieved by selecting Search Recipes).  When you want to start a new recipe you can select Start New Recipe from the menu, this will clear your current ingredients and start your recipe over.\n\n" +
                "You also have the option to save your recipe to a text file in case you want to print out your recipe, select this option from the menu.  If at any point in time the properties of an ingredient has changed, for example price, you can select Search/Edit Ingredients from the menu to look up your ingredient and make changes to the item.  Finally, you have the option to load in your list of ingredients from a csv file.  Just select Import Ingredients from a CSV File from the menu, select your file, and the list of ingredients in the csv file will replace your current ingredient archive saved by the program.  The csv file must have 15 fields of data per line for each ingredient seperated by a comma.  The fields, in order, are: name, serving size quantity, serving size measurement type, servings per container, calories, total fat, saturated fat, trans fat, cholesterol, sodium, carbs, fiber, sugar, protein, price per package.  A new line must seperate each ingredient.";

            MessageBox.Show(msg);
        }

        private void menuItem_About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Nutrition Calculator\n\nVersion: 1.0\nMade by Richard Teller\n");
        }
        #endregion
        #endregion

        #region Client Area Click Events
        private void button_AddAnotherIngredient_Click(object sender, RoutedEventArgs e)
        {
            AddIngredientGrid();
        }

        public void button_ConfirmIngredients_Click(object sender, RoutedEventArgs e)
        {
            currentRecipe = new Recipe();

            bool abortCalculation = false, validIngredient = false, servingsSpecified = true, measuredByVolume = false;
            double portion, ingrQtyVal = 0; // portion represents how much the ingredient used is compared to one serving size of that ingredient
            double n_servingsMade = 0;


            if (textBox_NumberServingsMade.Text == "")
                servingsSpecified = false;
            else
            {
                try
                {
                    n_servingsMade = Double.Parse(textBox_NumberServingsMade.Text);

                    if (n_servingsMade <= 0)
                    {
                        abortCalculation = true;
                        MessageBox.Show("A number <= 0 was detected.");
                    }
                }
                catch
                {
                    MessageBox.Show("Alpha input detected for number of servings.");
                    abortCalculation = true;
                }
            }

            double totalCalories = 0, totalFat = 0, totalSatFat = 0, totalTransFat = 0, totalCholesterol = 0,
                totalSodium = 0, totalCarbs = 0, totalFiber = 0, totalSugar = 0, totalProtein = 0, totalPrice = 0;

            // add nutrition from each ingredient entered
            foreach (Grid g in recipeIngredientList)
            {
                if (abortCalculation == false) // an ingredient containing invalid fields has not been found yet
                {
                    UIElementCollection gridChildren = g.Children;
                    TextBox ingrQty = (TextBox)gridChildren[0];
                    ComboBox ingrMsr = (ComboBox)gridChildren[1];
                    ComboBox ingrIngredient = (ComboBox)gridChildren[3];

                    if (ingrQty.Text == "" && ingrMsr.Text == "" && ingrIngredient.Text == "")
                    {
                        // skip adding this ingredient since it was left blank but do not abort
                    }
                    else if (ingrQty.Text == "" || ingrMsr.Text == "" || ingrIngredient.Text == "")
                    {
                        // not all fields were left blank so user forgot a field, abort the calculation and show message
                        abortCalculation = true;

                        String msg = "An empty field was detected for an ingredient.  Either\n"
                            + "occupy all fields or leave them all blank.";
                        MessageBox.Show(msg);
                    }
                    else
                    {
                        // all fields were entered for the ingredient, now check if they are valid
                        try
                        {
                            ingrQtyVal = Double.Parse(ingrQty.Text);

                            if (ingrQtyVal <= 0)
                            {
                                abortCalculation = true;
                                MessageBox.Show("A number <= 0 was detected.");
                            }

                        }
                        catch (Exception)
                        {
                            abortCalculation = true;
                            MessageBox.Show("Alpha input detected for number of servings.");
                        }


                        String ingrMsr_AsString = ingrMsr.Text;
                        Ingredient ingredient = (Ingredient)ingrIngredient.SelectedItem;

                        if (ingrMsr_AsString == "oz" || ingrMsr_AsString == "mg" || ingrMsr_AsString == "g" || ingrMsr_AsString == "kg")
                            measuredByVolume = false;
                        else
                            measuredByVolume = true;

                        if (measuredByVolume != ingredient.measuredByVolume)
                        {
                            String msg = "Invalid Measurement Type Conversion\n\n";

                            msg += "Recipe calls for " + ingrQty.Text + " " + ingrMsr.Text + " of " + ingredient.ToString() +
                                " but " + ingredient.ToString() + " is currently only known to be measured by " + ingredient.ServingMsr + ".\n";

                            msg += "Cannot convert a ";
                            if (measuredByVolume == true)
                                msg += "volume";
                            else
                                msg += "mass";

                            msg += " measurement to a ";
                            if (ingredient.measuredByVolume == true)
                                msg += "volume";
                            else
                                msg += "mass";

                            msg += " measurement.";

                            MessageBox.Show(msg);

                            abortCalculation = true;
                        }

                        // All fields have been gathered and tested for validity, calculate if we can
                        if (abortCalculation == false)
                        {
                            validIngredient = true; // atleast one ingredient was valid

                            portion = GetPortionSize(ingrQtyVal, ingrMsr_AsString, ingredient);

                            totalCalories += ingredient.Calories * portion;
                            totalFat += ingredient.Fat * portion;
                            totalSatFat += ingredient.SatFat * portion;
                            totalTransFat += ingredient.TransFat * portion;
                            totalCholesterol += ingredient.Cholesterol * portion;
                            totalSodium += ingredient.Sodium * portion;
                            totalCarbs += ingredient.Carbs * portion;
                            totalFiber += ingredient.Fiber * portion;
                            totalSugar += ingredient.Sugar * portion;
                            totalProtein += ingredient.Protein * portion;

                            double pricePerServing = ingredient.Price / ingredient.ServingPerContainer;
                            totalPrice += pricePerServing * portion;

                            // now add the ingredient to the recipe
                            RecipeIngredient rIngredient = new RecipeIngredient(ingrQtyVal, ingrMsr_AsString, ingredient);
                            currentRecipe.ingredients.Add(rIngredient);
                        }
                    }
                }
            }

            label_ServingCalories.Content = "-";
            label_ServingFat.Content = "-";
            label_ServingSatFat.Content = "-";
            label_ServingTransFat.Content = "-";
            label_ServingCholesterol.Content = "-";
            label_ServingSodium.Content = "-";
            label_ServingCarbs.Content = "-";
            label_ServingFiber.Content = "-";
            label_ServingSugar.Content = "-";
            label_ServingProtein.Content = "-";
            label_ServingPrice.Content = "-";

            label_RecipeCalories.Content = "-";
            label_RecipeFat.Content = "-";
            label_RecipeSatFat.Content = "-";
            label_RecipeTransFat.Content = "-";
            label_RecipeCholesterol.Content = "-";
            label_RecipeSodium.Content = "-";
            label_RecipeCarbs.Content = "-";
            label_RecipeFiber.Content = "-";
            label_RecipeSugar.Content = "-";
            label_RecipeProtein.Content = "-";
            label_RecipePrice.Content = "-";

            textBox_RecipeName.Text = "Add a Recipe Name (Click me)";
            textBox_RecipeIngredients.Text = "Ingredients will be added here";
            textBox_RecipeDirections.Text = "Add directions here.";
            textBox_RecipeNotes.Text = "Add notes here.";

            if (validIngredient && abortCalculation == false)
            {
                // update nutrition facts label to hold the values
                label_RecipeCalories.Content = String.Format("{0:0.#}", totalCalories);
                label_RecipeFat.Content = String.Format("{0:0.#}", totalFat);
                label_RecipeSatFat.Content = String.Format("{0:0.#}", totalSatFat);
                label_RecipeTransFat.Content = String.Format("{0:0.#}", totalTransFat);
                label_RecipeCholesterol.Content = String.Format("{0:0.#}", totalCholesterol);
                label_RecipeSodium.Content = String.Format("{0:0.#}", totalSodium);
                label_RecipeCarbs.Content = String.Format("{0:0.#}", totalCarbs);
                label_RecipeFiber.Content = String.Format("{0:0.#}", totalFiber);
                label_RecipeSugar.Content = String.Format("{0:0.#}", totalSugar);
                label_RecipeProtein.Content = String.Format("{0:0.#}", totalProtein);
                label_RecipePrice.Content = "$" + String.Format("{0:0.00}", totalPrice);

                if (servingsSpecified == true) // user did specify how many servings the recipe made (and it is > 0)
                {
                    label_ServingCalories.Content = String.Format("{0:0.#}", totalCalories / n_servingsMade);
                    label_ServingFat.Content = String.Format("{0:0.#}", totalFat / n_servingsMade);
                    label_ServingSatFat.Content = String.Format("{0:0.#}", totalSatFat / n_servingsMade);
                    label_ServingTransFat.Content = String.Format("{0:0.#}", totalTransFat / n_servingsMade);
                    label_ServingCholesterol.Content = String.Format("{0:0.#}", totalCholesterol / n_servingsMade);
                    label_ServingSodium.Content = String.Format("{0:0.#}", totalSodium / n_servingsMade);
                    label_ServingCarbs.Content = String.Format("{0:0.#}", totalCarbs / n_servingsMade);
                    label_ServingFiber.Content = String.Format("{0:0.#}", totalFiber / n_servingsMade);
                    label_ServingSugar.Content = String.Format("{0:0.#}", totalSugar / n_servingsMade);
                    label_ServingProtein.Content = String.Format("{0:0.#}", totalProtein / n_servingsMade);
                    label_ServingPrice.Content = "$" + String.Format("{0:0.00}", totalPrice / n_servingsMade);
                }
                else // user did not specify a number of servings so just show them the recipe amount automatically
                {
                    if (displayingNutrition == true)
                    {
                        // execute TogglePerRecipe_MouseDown event
                        if (displayingPerServing == true)
                        {
                            // want to show per recipe
                            displayingPerServing = false;

                            label_TogglePerServing.Opacity = 0.6;
                            label_TogglePerRecipe.Opacity = 1;

                            stackPanel_NutritionLabelPerServing.Visibility = Visibility.Collapsed;
                            stackPanel_NutritionLabelPerRecipe.Visibility = Visibility.Visible;
                        }
                    }
                }

                // update recipe panel
                String text = "";
                foreach (RecipeIngredient rIngredient in currentRecipe.ingredients)
                {
                    text += " - " + rIngredient.Quantity.ToString() + " " + rIngredient.MeasureType + " " + rIngredient.Ingredient.ToString() + "\n";
                }
                textBox_RecipeIngredients.Text = text.Substring(0, text.Length - 1); // remove last \n

                if (servingsSpecified)
                {
                    textBox_RecipeNotes.Text = "Makes " + n_servingsMade.ToString() + " servings.";
                    currentRecipe.Notes = "Makes " + n_servingsMade.ToString() + " servings.";
                }
            }
            else
                currentRecipe = new Recipe();

        }

        private void label_NutritionFacts_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (displayingNutrition == false)
            {
                // want to show nutrition label
                displayingNutrition = true;

                label_NutritionFacts.Opacity = 1;
                label_Recipe.Opacity = 0.6;

                grid_AmtPerServingRecipeToggle.Visibility = Visibility.Visible;
                // show the correct nutrition label
                if (displayingPerServing)
                {
                    // show label for serving
                    stackPanel_NutritionLabelPerServing.Visibility = Visibility.Visible;
                    stackPanel_NutritionLabelPerRecipe.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // show label for recipe
                    stackPanel_NutritionLabelPerServing.Visibility = Visibility.Collapsed;
                    stackPanel_NutritionLabelPerRecipe.Visibility = Visibility.Visible;
                }

                dockPanel_Recipe.Visibility = Visibility.Collapsed;
            }
        }

        private void label_TogglePerServing_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (displayingPerServing == false)
            {
                // want to show per serving
                displayingPerServing = true;

                label_TogglePerServing.Opacity = 1;
                label_TogglePerRecipe.Opacity = 0.6;

                stackPanel_NutritionLabelPerServing.Visibility = Visibility.Visible;
                stackPanel_NutritionLabelPerRecipe.Visibility = Visibility.Collapsed;
            }
        }

        private void label_TogglePerRecipe_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (displayingPerServing == true)
            {
                // want to show per recipe
                displayingPerServing = false;

                label_TogglePerServing.Opacity = 0.6;
                label_TogglePerRecipe.Opacity = 1;

                stackPanel_NutritionLabelPerServing.Visibility = Visibility.Collapsed;
                stackPanel_NutritionLabelPerRecipe.Visibility = Visibility.Visible;
            }
        }

        private void label_Recipe_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (displayingNutrition == true)
            {
                // want to show recipe
                displayingNutrition = false;

                label_NutritionFacts.Opacity = 0.6;
                label_Recipe.Opacity = 1;

                grid_AmtPerServingRecipeToggle.Visibility = Visibility.Collapsed;
                stackPanel_NutritionLabelPerServing.Visibility = Visibility.Collapsed;
                stackPanel_NutritionLabelPerRecipe.Visibility = Visibility.Collapsed;
                dockPanel_Recipe.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region Helper Functions
        public void AddIngredientGrid()
        {
            n_currentIngredients++;

            // create the grid
            Grid grid = new Grid();
            grid.Margin = new Thickness(0, 5, 0, 0);
            grid.Height = 26;

            // create the input fields
            TextBox ingr_qty = new TextBox();
            ingr_qty.Height = 22;
            ingr_qty.Width = 32;
            ingr_qty.HorizontalAlignment = HorizontalAlignment.Left;
            ingr_qty.Margin = new Thickness(10, 0, 0, 0);
            ingr_qty.HorizontalContentAlignment = HorizontalAlignment.Right;

            Separator separator = new Separator();
            separator.IsEnabled = false;
            separator.Margin = new Thickness(0, 5, 0, 5);
            separator.Padding = new Thickness(0, 0, 0, 0);

            ComboBox ingr_msr = new ComboBox();
            ingr_msr.Height = 22;
            ingr_msr.Width = 60;
            ingr_msr.HorizontalAlignment = HorizontalAlignment.Left;
            ingr_msr.Margin = new Thickness(50, 0, 0, 0);

            ingr_msr.Items.Add("");
            ingr_msr.Items.Add("tsp.");
            ingr_msr.Items.Add("tbsp.");
            ingr_msr.Items.Add("fl oz");
            ingr_msr.Items.Add("cup");
            ingr_msr.Items.Add("pint");
            ingr_msr.Items.Add("quart");
            ingr_msr.Items.Add("gallon");
            ingr_msr.Items.Add("ml");
            ingr_msr.Items.Add("liter");
            ingr_msr.Items.Add(separator);
            ingr_msr.Items.Add("oz");
            ingr_msr.Items.Add("mg");
            ingr_msr.Items.Add("g");
            ingr_msr.Items.Add("kg");

            Label label_of = new Label();
            label_of.Content = "of";
            label_of.Height = 26;
            label_of.Width = 20;
            label_of.HorizontalAlignment = HorizontalAlignment.Left;
            label_of.Margin = new Thickness(110, 0, 0, 0);
            label_of.Padding = new Thickness(4, 5, 0, 5);

            ComboBox ingr_ingredient = new ComboBox();
            ingr_ingredient.Height = 22;
            ingr_ingredient.Width = 150;
            ingr_ingredient.HorizontalAlignment = HorizontalAlignment.Left;
            ingr_ingredient.Margin = new Thickness(130, 0, 0, 0);
            foreach (Ingredient i in ingredientDatabaseList)
            {
                ingr_ingredient.Items.Add(i);
            }

            // add the input fields to the grid
            grid.Children.Add(ingr_qty);
            grid.Children.Add(ingr_msr);
            grid.Children.Add(label_of);
            grid.Children.Add(ingr_ingredient);

            // add the grid to the list datastructure
            recipeIngredientList.Add(grid);

            // add the grid to the window
            stackPanel_Ingredients.Children.Add(grid);
        }

        private double GetPortionSize(double ingrQty, String ingrMsr, Ingredient ingredient)
        {
            bool measuredByVolume = false;
            double amt_InGrams = 0, amt_InCups = 0;
            double ratio = 0, conversionFactor = 0;

            if (ingrMsr == "oz" || ingrMsr == "mg" || ingrMsr == "g" || ingrMsr == "kg")
                measuredByVolume = false;
            else
                measuredByVolume = true;

            if (measuredByVolume == true)
            {
                switch (ingrMsr) // get the conversion factor for converting the type to cups (for example, 16 tbsp = 1 cup)
                {
                    case "tsp.":
                        conversionFactor = 48;
                        break;

                    case "tbsp.":
                        conversionFactor = 16;
                        break;

                    case "fl oz":
                        conversionFactor = 8;
                        break;

                    case "cup":
                        conversionFactor = 1;
                        break;

                    case "pint":
                        conversionFactor = 0.5;
                        break;

                    case "quart":
                        conversionFactor = 0.25;
                        break;

                    case "gallon":
                        conversionFactor = 0.0625;
                        break;

                    case "ml":
                        conversionFactor = 236.588237;
                        break;

                    case "liter":
                        conversionFactor = 0.23658824;
                        break;
                }

                amt_InCups = ingrQty / conversionFactor;
                ratio = amt_InCups / ingredient.ServingQty_True;
            }
            else // measured by mass
            {
                switch (ingrMsr) // get conversion factor for the type to grams
                {
                    case "oz":
                        conversionFactor = 0.03527396;
                        break;

                    case "mg":
                        conversionFactor = 1000;
                        break;

                    case "g":
                        conversionFactor = 1;
                        break;

                    case "kg":
                        conversionFactor = 0.001;
                        break;
                }

                amt_InGrams = ingrQty / conversionFactor;
                ratio = amt_InGrams / ingredient.ServingQty_True;
            }

            return ratio;
        }

        private void RefreshIngredientDropdowns()
        {
            ingredientDatabaseList.Sort();

            // clear current dropdown of ingredients
            foreach (Grid g in recipeIngredientList)
            {
                UIElementCollection collection = g.Children;
                ComboBox comboBox = (ComboBox)collection[3]; // the ingredient combobox is the 4th item in the grid
                comboBox.Items.Clear();

                // update ingredient dropdown
                foreach (Ingredient i in ingredientDatabaseList)
                {
                    comboBox.Items.Add(i);
                }
            }
        }


        #endregion
    }
}
