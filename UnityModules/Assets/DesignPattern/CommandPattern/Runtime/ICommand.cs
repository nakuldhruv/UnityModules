namespace Nakul.DesignPattern
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}