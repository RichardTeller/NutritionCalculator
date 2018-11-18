using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutritionCalculator
{
    /* A RecipeIngredient is used to represent an ingredient that is being used for a recipe.  Thus this class will contain how much of a particular ingredient
     * is being used.  For example 2 tbsp of Jif Peanut Butter:  The RecipeIngredient object would contain 2, tbsp, and Jif Peanut Butter, for Quantity, MeasureType
     * and Ingredient respectively.
     */
    public class RecipeIngredient
    {
        public Ingredient Ingredient;
        public double Quantity;
        public String MeasureType;

        public RecipeIngredient()
        {
            Quantity = 0;
            MeasureType = "";
            Ingredient = new Ingredient();
        }
        public RecipeIngredient(double q, String m, Ingredient i)
        {
            Quantity = q;
            MeasureType = m;
            Ingredient = i;
        }
    }
}
