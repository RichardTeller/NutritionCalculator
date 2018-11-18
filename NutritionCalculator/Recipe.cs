using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutritionCalculator
{
    public class Recipe : IComparable<Recipe>
    {
        public String Name, Directions, Notes;
        public List<RecipeIngredient> ingredients;

        public Recipe()
        {
            Name = "Add a Recipe Name (Click me)";
            Directions = "Add directions here.";
            Notes = "Add notes here.";

            ingredients = new List<RecipeIngredient>();
        }

        public Recipe(String name, String directions, String notes)
        {
            Name = name;
            Directions = directions;
            Notes = notes;

            ingredients = new List<RecipeIngredient>();
        }

        public int CompareTo(Recipe other)
        {
            return this.Name.CompareTo(other.Name);
        }

        public bool Equals(Recipe other)
        {
            if (this.Name.CompareTo(other.Name) == 0)
                return true;

            return false;
        }

        public override String ToString()
        {
            return Name;
        }
    }
}
