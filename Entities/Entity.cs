// henrique agostinetto piva
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Entities;

// classe base para entidades
public abstract class Entity
{
    public int Id { get; protected set; }
    protected Entity(int id = 0)
    {
        if (id < 0) throw new DomainException("ID_NEGATIVO");
        Id = id;
    }
}
