using CaminhoLivre.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CaminhoLivre.Api.Data;

// Dados de teste usados no modo SQLite. No Oracle os dados vêm do script-bd.sql do grupo.
public static class DbSeed
{
    public static async Task PopularAsync(AppDbContext db)
    {
        if (await db.Orgaos.AnyAsync())
        {
            return;
        }

        var obras = new Orgao { Id = 1, Nome = "Secretaria de Obras", Area = AreaOrgao.OBRAS };
        var iluminacao = new Orgao { Id = 2, Nome = "Departamento de Iluminação Pública", Area = AreaOrgao.ILUMINACAO };
        var mobilidade = new Orgao { Id = 3, Nome = "Secretaria de Mobilidade", Area = AreaOrgao.MOBILIDADE };
        var limpeza = new Orgao { Id = 4, Nome = "Serviço de Limpeza Urbana", Area = AreaOrgao.LIMPEZA };
        db.Orgaos.AddRange(obras, iluminacao, mobilidade, limpeza);

        db.Categorias.AddRange(
            new Categoria { Id = 1, Codigo = "CALCADA", Nome = "Calçada danificada", OrgaoId = obras.Id },
            new Categoria { Id = 2, Codigo = "RAMPA", Nome = "Rampa ausente ou obstruída", OrgaoId = obras.Id },
            new Categoria { Id = 3, Codigo = "BURACO", Nome = "Buraco na via", OrgaoId = obras.Id },
            new Categoria { Id = 4, Codigo = "ILUMINACAO", Nome = "Poste apagado", OrgaoId = iluminacao.Id },
            new Categoria { Id = 5, Codigo = "SEMAFORO_SONORO", Nome = "Semáforo sonoro quebrado", OrgaoId = mobilidade.Id },
            new Categoria { Id = 6, Codigo = "PISO_TATIL", Nome = "Piso tátil danificado", OrgaoId = mobilidade.Id },
            new Categoria { Id = 7, Codigo = "LIXO", Nome = "Lixo ou entulho", OrgaoId = limpeza.Id },
            new Categoria { Id = 8, Codigo = "OUTRO", Nome = "Outro problema", OrgaoId = obras.Id });

        db.Usuarios.AddRange(
            new Usuario { Id = 1, Nome = "Maria Souza", Email = "maria@teste.com", Role = Papeis.Cidadao },
            new Usuario { Id = 2, Nome = "João Lima", Email = "joao@teste.com", Role = Papeis.Cidadao },
            new Usuario { Id = 3, Nome = "Carlos Gestor Obras", Email = "gestor.obras@teste.com", Role = Papeis.Gestor, OrgaoId = obras.Id },
            new Usuario { Id = 4, Nome = "Ana Gestora Iluminação", Email = "gestor.iluminacao@teste.com", Role = Papeis.Gestor, OrgaoId = iluminacao.Id });

        await db.SaveChangesAsync();
    }
}
