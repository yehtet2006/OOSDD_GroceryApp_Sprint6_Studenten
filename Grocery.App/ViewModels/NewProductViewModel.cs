using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;


namespace Grocery.App.ViewModels;

public partial class NewProductViewModel : BaseViewModel
{
    private readonly IProductService _productService;
    public string Name { get; set; }
    public int Stock { get; set; }
    public DateOnly Shelflife { get; set; }
    public decimal Price { get; set; }
    
    [ObservableProperty]
    private string errorMessage;

    [ObservableProperty]
    private string message;
    
    public event Action? OnProductAdd;
    
    public NewProductViewModel(IProductService productService)
    {
        _productService = productService;
        Shelflife = DateOnly.FromDateTime(DateTime.Today);
    }

    [RelayCommand]
    public async Task AddProduct()
    {
        string ErrorMessage = "";
        string Message = "";
        if (_productService.ProductExists(Name))
        {
            ErrorMessage = "Name already exist.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Name is required.";
            return;
        }

        if (Stock < 0)
        {
            ErrorMessage = "Stock is negative.";
            return;
        }
        if (Price < 0)
        {
            ErrorMessage = "Price is negative.";
            return;
        }
        Product product = new Product(0, Name, Stock, Shelflife, Price);
        _productService.Add(product);
        Message = $"Product {Name} added.";
        OnProductAdd?.Invoke();
        await Shell.Current.GoToAsync("..");


    }
    
}