using ParlourPro.ViewModels;

namespace ParlourPro.Views;

public partial class DashboardPage : ContentPage
{
	public DashboardPage(DashboardViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is DashboardViewModel vm)
        {
            // Ensure earnings are recalculated whenever the user navigates back to dashboard
            await vm.LoadEarnings();
        }
    }

}