﻿using Microsoft.Bot.Builder.Form;
using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable 649

// The SandwichOrder is the simple form you want to fill out.  It must be serializable so the bot can be stateless.
// The order of fields defines the default order in which questions will be asked.
// Enumerations shows the legal options for each field in the SandwichOrder and the order is the order values will be presented 
// in a conversation.
namespace Microsoft.Bot.Sample.AnnotatedSandwichBot
{
    public enum SandwichOptions
    {
        BLT, BlackForestHam, BuffaloChicken, ChickenAndBaconRanchMelt, ColdCutCombo, MeatballMarinara,
        OverRoastedChicken, RoastBeef,
        [Terms(@"rotis\w* style chicken", MaxPhrase = 3)]
        RotisserieStyleChicken, SpicyItalian, SteakAndCheese, SweetOnionTeriyaki, Tuna,
        TurkeyBreast, Veggie
    };
    public enum LengthOptions { SixInch, FootLong };
    public enum BreadOptions { NineGrainWheat, NineGrainHoneyOat, Italian, ItalianHerbsAndCheese, Flatbread };
    public enum CheeseOptions { American, MontereyCheddar, Pepperjack };
    public enum ToppingOptions
    {
        // This starts at 1 because 0 is the "no value" value
        [Terms("except", "but", "not", "no", "all", "everything")]
        Everything = 1,
        Avocado, BananaPeppers, Cucumbers, GreenBellPeppers, Jalapenos,
        Lettuce, Olives, Pickles, RedOnion, Spinach, Tomatoes
    };
    public enum SauceOptions
    {
        ChipotleSouthwest, HoneyMustard, LightMayonnaise, RegularMayonnaise,
        Mustard, Oil, Pepper, Ranch, SweetOnion, Vinegar
    };

    [Serializable]
    [Template(TemplateUsage.NotUnderstood, "I do not understand \"{0}\".", "Try again, I don't get \"{0}\".")]
    [Template(TemplateUsage.EnumSelectOne, "What kind of {&} would you like on your sandwich? {||}", ChoiceStyle = ChoiceStyleOptions.PerLine)]
    class SandwichOrder
    {
        [Prompt("What kind of {&} would you like? {||}")]
        // [Prompt("What kind of {&} would you like? {||}", ChoiceFormat ="{1}")]
        // [Prompt("What kind of {&} would you like?")]
        public SandwichOptions? Sandwich;

        [Prompt("What size of sandwich do you want? {||}")]
        public LengthOptions? Length;

        public BreadOptions? Bread;

        // An optional annotation means that it is possible to not make a choice in the field.
        [Optional]
        public CheeseOptions? Cheese;

        [Optional]
        public List<ToppingOptions> Toppings
        {
            get { return _toppings; }
            set
            {
                if (value.Contains(ToppingOptions.Everything))
                {
                    _toppings = (from ToppingOptions topping in Enum.GetValues(typeof(ToppingOptions))
                                 where topping != ToppingOptions.Everything && !value.Contains(topping)
                                 select topping).ToList();
                }
                else
                {
                    _toppings = value;
                }
            }
        }
        private List<ToppingOptions> _toppings;

        [Optional]
        public List<SauceOptions> Sauces;

        public string DeliveryAddress;

        [Optional]
        public DateTime? DeliveryTime;

        [Numeric(1, 5)]
        [Optional]
        [Describe("your experience today")]
        public double? Rating;

        public static IForm<SandwichOrder> BuildForm()
        {
            return new FormBuilder<SandwichOrder>()
                        .Message("Welcome to the sandwich order bot!")
                        .Field(nameof(SandwichOrder.Sandwich))
                        .Field(nameof(SandwichOrder.Length))
                        .Field(nameof(SandwichOrder.Bread))
                        .Field(nameof(SandwichOrder.Cheese))
                        .Field(nameof(SandwichOrder.Toppings))
                        .Message("For sandwich toppings you have selected {Toppings}.")
                        .Field(nameof(SandwichOrder.Sauces))
                        .Field(nameof(SandwichOrder.DeliveryAddress),
                            validate: async (state, response) =>
                            {
                                var result = new ValidateResult { IsValid = true };
                                var address = (response as string).Trim();
                                if (address.Length > 0 && address[0] < '0' || address[0] > '9')
                                {
                                    result.Feedback = "Address must start with a number.";
                                    result.IsValid = false;
                                }
                                return result;
                            })
                        .Field(nameof(SandwichOrder.DeliveryTime), "What time do you want your sandwich delivered? {||}")
                        .Confirm("Do you want to order your {Length} {Sandwich} on {Bread} {&Bread} with {[{Cheese} {Toppings} {Sauces}]} to be sent to {DeliveryAddress} {?at {DeliveryTime:t}}?")
                        .AddRemainingFields()
                        .Message("Thanks for ordering a sandwich!")
                        .Build();
        }
    };
}