using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Pokemon.Models
{
    public class PokemonMasterResponse
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        public string Nome { get; set; }
        [Required(ErrorMessage = "O campo Usuario é obrigatório.")]
        public string Usuario { get; set; }

        [JsonIgnore]
        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        public string Senha { get; set; }
    }
}
