using SQLite;

namespace AvaliaRBI._3___Domain.Abstractions
{
    public abstract class BaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}
