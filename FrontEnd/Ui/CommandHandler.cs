using System.Windows.Input;

namespace FrontEnd.Ui;

/// <summary>
/// Creates instance of the command handler
/// </summary>
/// <param name="action">Action to be executed by the command</param>
/// <param name="canExecute">A bolean property to containing current permissions to execute the command</param>
public class CommandHandler(Action<object?> action, Func<bool> canExecute) : ICommand
{
    /// <summary>
    /// Wires CanExecuteChanged event 
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Forcess checking if execute is allowed
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public bool CanExecute(object? parameter)
    {
        return canExecute.Invoke();
    }

    /// <summary>
    /// Execute the command
    /// </summary>
    /// <param name="parameter">Optional parameter for the command</param>
    public void Execute(object? parameter)
    {
        action(parameter);
    }
}