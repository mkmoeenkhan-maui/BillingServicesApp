using ParlourPro.ViewModels;

namespace ParlourPro.Views;

public partial class BillDetailsPage : ContentPage
{
	public BillDetailsPage(BillDetailsViewModel billDetailsViewModel)
	{
		InitializeComponent();
		BindingContext = billDetailsViewModel;
	}
}