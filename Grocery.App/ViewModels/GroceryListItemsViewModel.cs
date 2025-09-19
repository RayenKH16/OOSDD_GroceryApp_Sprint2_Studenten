using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;

namespace Grocery.App.ViewModels
{
    [QueryProperty(nameof(GroceryList), nameof(GroceryList))]
    public partial class GroceryListItemsViewModel : BaseViewModel
    {
        private readonly IGroceryListItemsService _groceryListItemsService;
        private readonly IProductService _productService;
        public ObservableCollection<GroceryListItem> MyGroceryListItems { get; set; } = [];
        public ObservableCollection<Product> AvailableProducts { get; set; } = [];

        [ObservableProperty]
        GroceryList groceryList = new(0, "None", DateOnly.MinValue, "", 0);

        public GroceryListItemsViewModel(IGroceryListItemsService groceryListItemsService, IProductService productService)
        {
            _groceryListItemsService = groceryListItemsService;
            _productService = productService;
            Load(groceryList.Id);
        }

        private void Load(int id)
        {
            MyGroceryListItems.Clear();
            foreach (var item in _groceryListItemsService.GetAllOnGroceryListId(id))
                MyGroceryListItems.Add(item);

            GetAvailableProducts();
        }

        private void GetAvailableProducts()
        {
            AvailableProducts.Clear();

            // Haal alle producten op
            var allProducts = _productService.GetAll();

            // Filter: product moet voorraad hebben (>0) en mag niet al op de boodschappenlijst staan
            foreach (var product in allProducts)
            {
                bool alreadyOnList = MyGroceryListItems.Any(item => item.ProductId == product.Id);
                if (product.Stock > 0 && !alreadyOnList)
                {
                    AvailableProducts.Add(product);
                }
            }
        }

        partial void OnGroceryListChanged(GroceryList value)
        {
            Load(value.Id);
        }

        [RelayCommand]
        public async Task ChangeColor()
        {
            Dictionary<string, object> paramater = new() { { nameof(GroceryList), GroceryList } };
            await Shell.Current.GoToAsync($"{nameof(ChangeColorView)}?Name={GroceryList.Name}", true, paramater);
        }

        [RelayCommand]
        public void AddProduct(Product product)
        {
            if (product == null || product.Id <= 0) return;

            // Maak een nieuw GroceryListItem
            var newItem = new GroceryListItem(
                id: 0,
                groceryListId: GroceryList.Id,
                productId: product.Id,
                amount: 1 // standaard hoeveelheid
            );

            // Voeg het item toe via de service
            _groceryListItemsService.Add(newItem);

            // Verminder de voorraad van het product en sla op
            product.Stock -= 1;
            _productService.Update(product);

            // Werk de viewmodels bij
            OnGroceryListChanged(GroceryList);
        }
    }
}
