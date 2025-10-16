using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;

namespace Grocery.App.ViewModels
{
    public partial class ProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        public ObservableCollection<Product> Products { get; set; }
        public Client Client { get; set; }
        

        public ProductViewModel(IProductService productService, GlobalViewModel globalViewModel)
        {
            _productService = productService;
            Client = globalViewModel.Client;
            Products = [];
            foreach (Product p in _productService.GetAll()) Products.Add(p);
        }
        public void RefreshProducts()
        {
            Products.Clear();
            foreach (Product p in _productService.GetAll())
            {
                Products.Add(p);
            }
        }
        
        [RelayCommand]
        public async Task NewProduct()
        {
            if (Client.Role == Role.Admin)
            {
                await Shell.Current.GoToAsync(nameof(NewProductView));
                RefreshProducts();
            }
            
        }
    }
}
