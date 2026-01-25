using LetterboxdComparer.Entities;
using System.Collections.Generic;

namespace LetterboxdComparer.Data
{
    public interface ICRUDHandler
    {
        bool Create<T>(List<T> entities) where T : BaseEntity;
        List<T> Read<T>() where T : BaseEntity;
        bool Update<T>(List<T> entities);
        bool Delete<T>(List<T> entities);
    }
}
