using CaminhoLivre.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CaminhoLivre.Api.Data;

// Dados iniciais (órgãos, categorias e usuários de teste) usados quando a própria API cria as tabelas.
// No Oracle do grupo esses dados vêm do script-bd.sql.
public static class DbSeed
{
    public static async Task PopularAsync(AppDbContext db)
    {
        if (await db.Orgaos.AnyAsync())
        {
            return;
        }

        var obras = new Orgao { Nome = "Secretaria de Obras", Area = AreaOrgao.OBRAS };
        var iluminacao = new Orgao { Nome = "Departamento de Iluminação Pública", Area = AreaOrgao.ILUMINACAO };
        var mobilidade = new Orgao { Nome = "Secretaria de Mobilidade", Area = AreaOrgao.MOBILIDADE };
        var limpeza = new Orgao { Nome = "Serviço de Limpeza Urbana", Area = AreaOrgao.LIMPEZA };
        db.Orgaos.AddRange(obras, iluminacao, mobilidade, limpeza);

        db.Categorias.AddRange(
            new Categoria { Codigo = "CALCADA", Nome = "Calçada danificada", Orgao = obras },
            new Categoria { Codigo = "RAMPA", Nome = "Rampa ausente ou obstruída", Orgao = obras },
            new Categoria { Codigo = "BURACO", Nome = "Buraco na via", Orgao = obras },
            new Categoria { Codigo = "ILUMINACAO", Nome = "Poste apagado", Orgao = iluminacao },
            new Categoria { Codigo = "SEMAFORO_SONORO", Nome = "Semáforo sonoro quebrado", Orgao = mobilidade },
            new Categoria { Codigo = "PISO_TATIL", Nome = "Piso tátil danificado", Orgao = mobilidade },
            new Categoria { Codigo = "LIXO", Nome = "Lixo ou entulho", Orgao = limpeza },
            new Categoria { Codigo = "OUTRO", Nome = "Outro problema", Orgao = obras });

        db.Usuarios.AddRange(
            new Usuario { Nome = "Maria Souza", Email = "maria@teste.com", Role = Papeis.Cidadao },
            new Usuario { Nome = "João Lima", Email = "joao@teste.com", Role = Papeis.Cidadao },
            new Usuario { Nome = "Carlos Gestor Obras", Email = "gestor.obras@teste.com", Role = Papeis.Gestor, Orgao = obras },
            new Usuario { Nome = "Ana Gestora Iluminação", Email = "gestor.iluminacao@teste.com", Role = Papeis.Gestor, Orgao = iluminacao });

        // Sem IDs fixos: o banco gera (identity), o que funciona igual em SQLite, Oracle e SQL Server.
        await db.SaveChangesAsync();
    }
}
