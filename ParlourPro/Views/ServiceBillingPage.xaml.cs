using ParlourPro.ViewModels;

namespace ParlourPro.Views;

public partial class ServiceBillingPage : ContentPage
{
	public ServiceBillingPage(ServiceBillingViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}