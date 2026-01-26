
namespace LetterboxdComparer.ViewRelated
{
    /// <summary>
    /// Is used to notify presenters when they are activated (view switched to them).
    /// </summary>
    public interface IActivatable
    {
        void OnActivated();
    }
}
