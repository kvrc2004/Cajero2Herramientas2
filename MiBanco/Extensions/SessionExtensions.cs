using System.Text.Json;

namespace MiBanco.Extensions
{
    /// <summary>
    /// Extensiones para el manejo de sesiones en ASP.NET Core
    /// Permite serializar y deserializar objetos complejos en la sesión
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Establece un objeto en la sesión serializado como JSON
        /// </summary>
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        /// <summary>
        /// Obtiene un objeto de la sesión deserializado desde JSON
        /// </summary>
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonSerializer.Deserialize<T>(value);
        }

        /// <summary>
        /// Verifica si una clave existe en la sesión
        /// </summary>
        public static bool HasKey(this ISession session, string key)
        {
            return session.Keys.Contains(key);
        }
    }
}