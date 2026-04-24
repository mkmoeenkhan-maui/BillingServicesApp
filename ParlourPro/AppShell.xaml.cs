using ParlourPro.Views;

namespace ParlourPro
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes so GoToAsync can find them
            Routing.RegisterRoute(nameof(ServiceBillingPage), typeof(ServiceBillingPage));
            Routing.RegisterRoute(nameof(ServiceMasterPage), typeof(ServiceMasterPage));
            Routing.RegisterRoute(nameof(HistoryPage), typeof(HistoryPage));
            Routing.RegisterRoute(nameof(BillDetailsPage), typeof(BillDetailsPage));
        }
    }
}
