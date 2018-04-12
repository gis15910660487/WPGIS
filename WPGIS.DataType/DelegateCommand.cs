
using System;
using System.Windows.Input;

namespace WPGIS.DataType
{
    /// <summary>
    /// 命令绑定的基类
    /// </summary>
    public class DelegateCommand : ICommand
    {
        //A method prototype without return value.
        public Action<object> ExecuteCommand = null;
        //A method prototype return a bool type.
        public Func<object, bool> CanExecuteCommand = null;
        public event EventHandler CanExecuteChanged;

        public DelegateCommand()
        {
        }

        public DelegateCommand(Action<object> executeCmd)
        {
            ExecuteCommand = executeCmd;
        }

        /// <summary>
        /// 确认是否可执行
        /// </summary>
        public bool CanExecute(object parameter)
        {
            if (CanExecuteCommand != null)
            {
                return CanExecuteCommand(parameter);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        public void Execute(object parameter)
        {
            if (ExecuteCommand != null)
            {
                ExecuteCommand(parameter);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

    }
}
