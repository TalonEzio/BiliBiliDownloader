using System.Windows.Input;

namespace BiliBiliDownloader.Wpf.Command
{
	public class RelayCommand(Action<object?> executeAction, Predicate<object?>? canExecute = null)
		: ICommand
	{
		public event EventHandler? CanExecuteChanged;

		public bool CanExecute(object? parameter)
		{
			return canExecute != null && canExecute(parameter);
		}

		public void Execute(object? parameter)
		{
			executeAction.Invoke(parameter);
		}

		public void OnCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this,EventArgs.Empty);
		}
	}
}
