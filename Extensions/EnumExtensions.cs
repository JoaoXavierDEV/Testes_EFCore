using System.ComponentModel;
using System.Reflection;

namespace WebApplication1.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Pega a descrição do DataAnnotaion ou o Nome do campo
        /// </summary>
        /// <param name="value"></param>
        /// <returns>string</returns>
        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }







        public static TEnum ParseFromDescription<TEnum>(string description) where TEnum : struct, Enum
        {
            foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attr = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                .OfType<DescriptionAttribute>()
                                .FirstOrDefault();
                var name = attr?.Description ?? field.Name;
                if (string.Equals(name, description, System.StringComparison.OrdinalIgnoreCase))
                    return (TEnum)Enum.Parse(typeof(TEnum), field.Name);
            }
            throw new System.ArgumentException($"Valor de descrição inválido para {typeof(TEnum).Name}: '{description}'");
        }


    }
}
