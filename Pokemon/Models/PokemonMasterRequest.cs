using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pokemon.Models
{
    public class PokemonMasterRequest
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string? Nome { get; set; }        
        public string? Usuario { get; set; }
        [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[\W_]).{8,}$",ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula e um caractere especial.")]        
        public string? Senha { get; set; }
    }
}
