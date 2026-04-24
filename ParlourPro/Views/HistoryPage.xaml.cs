using ParlourPro.ViewModels;

namespace ParlourPro.Views;

public partial class HistoryPage : ContentPage
{
	public HistoryPage(HistoryViewModel historyViewModel)
	{
		InitializeComponent();
		BindingContext = historyViewModel;
	}
}