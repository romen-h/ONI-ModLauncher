using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ONIModLauncher
{
	public class BasicActionCommand : ICommand
	{
		private Func<object, bool> CanExecuteFunc;
		private Action<object> ExecuteFunc;

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter)
		{
			return CanExecuteFunc(parameter);
		}

		public void Execute(object parameter)
		{
			ExecuteFunc(parameter);
		}

		public BasicActionCommand(Action<object> exec, Func<object, bool> canExec)
		{
			CanExecuteFunc = canExec;
			ExecuteFunc = exec;
		}
	}
}
