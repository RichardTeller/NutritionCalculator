using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutritionCalculator
{
    public class Ingredient : IComparable<Ingredient>
    {
        public bool measuredByVolume; // either measured by volume or mass
        public double ServingQty_True, conversionFactor;

        public String Name, ServingMsr;
        public double ServingQty, ServingPerContainer, Calories, Fat, SatFat, TransFat, Cholesterol,
            Sodium, Carbs, Fiber, Sugar, Protein, Price;

        public Ingredient()
        {
            this.Name = "";
            ServingQty = 0;
            ServingMsr = "";
            ServingPerContainer = 0;
            Calories = 0;
            Fat = 0;
            SatFat = 0;
            TransFat = 0;
            Cholesterol = 0;
            Sodium = 0;
            Carbs = 0;
            Fiber = 0;
            Sugar = 0;
            Protein = 0;
            Price = 0;

            measuredByVolume = true;
            ServingQty_True = 0;
            conversionFactor = 0;
        }

        public Ingredient(String name, double servingQty, String servingMsr, double servingPerContainer, double calories,
            double fat, double satFat, double transFat, double cholesterol, double sodium, double carbs, double fiber, double sugar,
            double protein, double price)
        {
            Name = name;
            ServingQty = servingQty;
            ServingMsr = servingMsr;
            ServingPerContainer = servingPerContainer;
            Calories = calories;
            Fat = fat;
            SatFat = satFat;
            TransFat = transFat;
            Cholesterol = cholesterol;
            Sodium = sodium;
            Carbs = carbs;
            Fiber = fiber;
            Sugar = sugar;
            Protein = protein;
            Price = price;

            // get standard measurement size
            if (ServingMsr == "oz" || ServingMsr == "mg" || ServingMsr == "g" || ServingMsr == "kg")
                measuredByVolume = false;
            else
                measuredByVolume = true;

            if (measuredByVolume) // convert to standard of cups
            {
                switch (ServingMsr)
                {
                    case "tsp.": // convert tsp to cup
                        ServingQty_True = servingQty / 48;
                        conversionFactor = 48;
                        break;

                    case "tbsp.":
                        ServingQty_True = servingQty / 16;
                        conversionFactor = 16;
                        break;

                    case "fl oz":
                        ServingQty_True = servingQty / 8;
                        conversionFactor = 8;
                        break;

                    case "cup":
                        ServingQty_True = servingQty;
                        conversionFactor = 1;
                        break;

                    case "pint":
                        ServingQty_True = servingQty / 0.5;
                        conversionFactor = 0.5;
                        break;

                    case "quart":
                        ServingQty_True = servingQty / 0.25;
                        conversionFactor = 0.25;
                        break;

                    case "gallon":
                        ServingQty_True = servingQty / 0.0625;
                        conversionFactor = 0.0625;
                        break;

                    case "ml":
                        ServingQty_True = servingQty / 236.588237;
                        conversionFactor = 236.588237;
                        break;

                    case "liter":
                        ServingQty_True = servingQty / 0.23658824;
                        conversionFactor = 0.23658824;
                        break;
                }
            }
            else // measured by mass, convert to standard of grams
            {
                switch (ServingMsr)
                {
                    case "oz":
                        ServingQty_True = servingQty / 0.03527396;
                        conversionFactor = 0.03527396;
                        break;

                    case "mg":
                        ServingQty_True = servingQty / 1000;
                        conversionFactor = 1000;
                        break;

                    case "g":
                        ServingQty_True = servingQty;
                        conversionFactor = 1;
                        break;

                    case "kg":
                        ServingQty_True = servingQty / 0.001;
                        conversionFactor = 0.001;
                        break;
                }
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public int CompareTo(Ingredient other)
        {
            return this.Name.CompareTo(other.Name);
        }

        public bool Equals(Ingredient other)
        {
            if (this.Name.CompareTo(other.Name) == 0)
                return true;

            else return false;
        }
    }
}
