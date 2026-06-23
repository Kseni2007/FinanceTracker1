namespace FinanceTracker.Services;

/// <summary>
/// Паттерн «Команда»: обратимое действие с операциями выполнения и отмены.
/// </summary>
public interface IUndoableAction
{
    string Caption { get; }
    void Do();
    void Undo();
}

/// <summary>Универсальное действие на основе делегатов.</summary>
public sealed class LambdaAction : IUndoableAction
{
    private readonly Action _do, _undo;
    public string Caption { get; }

    public LambdaAction(string caption, Action doIt, Action undoIt)
    {
        Caption = caption;
        _do = doIt;
        _undo = undoIt;
    }

    public void Do() => _do();
    public void Undo() => _undo();
}

/// <summary>
/// Распорядитель истории действий: хранит стеки выполненных и отменённых команд,
/// предоставляет операции отмены и повтора.
/// </summary>
public sealed class ActionStack
{
    private readonly Stack<IUndoableAction> _done = new();
    private readonly Stack<IUndoableAction> _undone = new();

    public event EventHandler? Changed;

    public bool CanUndo => _done.Count > 0;
    public bool CanRedo => _undone.Count > 0;
    public string UndoCaption => CanUndo ? _done.Peek().Caption : string.Empty;
    public string RedoCaption => CanRedo ? _undone.Peek().Caption : string.Empty;

    public void Push(IUndoableAction action)
    {
        action.Do();
        _done.Push(action);
        _undone.Clear();
        Journal.Note("Действие: " + action.Caption);
        Raise();
    }

    public void Undo()
    {
        if (!CanUndo) return;
        var a = _done.Pop();
        a.Undo();
        _undone.Push(a);
        Journal.Note("Отмена: " + a.Caption);
        Raise();
    }

    public void Redo()
    {
        if (!CanRedo) return;
        var a = _undone.Pop();
        a.Do();
        _done.Push(a);
        Journal.Note("Повтор: " + a.Caption);
        Raise();
    }

    private void Raise() => Changed?.Invoke(this, EventArgs.Empty);
}
