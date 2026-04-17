using ParlourPro.ViewModels;

namespace ParlourPro.Views;

public partial class ServiceMasterPage : ContentPage
{
	public ServiceMasterPage(ServiceMasterViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}