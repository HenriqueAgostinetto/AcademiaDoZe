// henrique agostinetto piva
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Application.Mappings;

public static class DatabaseTypeEnumMappingExtensions
{
    public static DatabaseType ToDatabaseType(this AppDatabaseType item) => (DatabaseType)item;
    public static AppDatabaseType ToAppDatabaseType(this DatabaseType item) => (AppDatabaseType)item;
}
