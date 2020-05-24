using System.ComponentModel.DataAnnotations;

namespace Shop.Models
{
    //[Table("Categoria")] como mudar o nome da tabela no EF 
    public class Category
    {   [Key]
        //[Column("Cat_Id")] como mudar o nome da coluna no EF 
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Este campo é obrigatório")]
        [MaxLength(60, ErrorMessage = "Este campo deve conter entre 3 e 60 caracteres")]
        [MinLength(3, ErrorMessage = "Este campo deve conter entre 3 e 60 caracteres")]
        public string Title { get; set; }
    }
}