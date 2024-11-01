using System;
using System.Collections.Generic;
using HorariosIPBejaMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace HorariosIPBejaMVC.Data
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ALUNO> ALUNOs { get; set; }

        public virtual DbSet<ANO_LETIVO> ANO_LETIVOs { get; set; }

        public virtual DbSet<CALENDARIO_ACADEMICO> CALENDARIO_ACADEMICOs { get; set; }

        public virtual DbSet<COORDENADOR_CURSO> COORDENADOR_CURSOs { get; set; }

        public virtual DbSet<CURSO> CURSOs { get; set; }

        public virtual DbSet<DOCENTE> DOCENTEs { get; set; }

        public virtual DbSet<ESCOLA> ESCOLAs { get; set; }

        public virtual DbSet<FUNCIONARIO> FUNCIONARIOs { get; set; }

        public virtual DbSet<HORARIO_REFERENCIAL> HORARIO_REFERENCIALs { get; set; }

        public virtual DbSet<HORARIO_SEMANAL> HORARIO_SEMANALs { get; set; }

        public virtual DbSet<INSTITUICAO> INSTITUICAOs { get; set; }

        public virtual DbSet<PERIODO_HORARIO> PERIODO_HORARIOs { get; set; }

        public virtual DbSet<SALA> SALAs { get; set; }

        public virtual DbSet<SEMANA> SEMANAs { get; set; }

        public virtual DbSet<TIPO_AULA> TIPO_AULAs { get; set; }

        public virtual DbSet<TIPO_CURSO> TIPO_CURSOs { get; set; }

        public virtual DbSet<TIPO_UTILIZADOR> TIPO_UTILIZADORs { get; set; }

        public virtual DbSet<TURMA> TURMAs { get; set; }

        public virtual DbSet<UC_DOCENTE> UC_DOCENTEs { get; set; }

        public virtual DbSet<UNIDADE_CURRICULAR> UNIDADE_CURRICULARs { get; set; }

        public virtual DbSet<UTILIZADOR> UTILIZADORs { get; set; }

        public virtual DbSet<ALUNO_UC> ALUNO_UCs { get; set; }

        public DbSet<SolucaoHorario> SolucaoHorarios { get; set; }

        public DbSet<SolucaoHorarioTemp> SolucaoHorarioTemps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ALUNO>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__ALUNO__3213E83F2F31CEA8");

                entity.Property(e => e.id).ValueGeneratedNever();

                entity.HasOne(d => d.curso)
                      .WithMany(p => p.ALUNOs)
                      .HasConstraintName("FK__ALUNO__curso_id__66603565");

                entity.HasOne(d => d.idNavigation)
                      .WithOne(p => p.ALUNO)
                      .HasConstraintName("FK__ALUNO__id__656C112C");

                // Remover a configuração implícita de muitos-para-muitos
                // entity.HasMany(d => d.ucs).WithMany(p => p.alunos)
                //     .UsingEntity<Dictionary<string, object>>(
                //         "ALUNO_UC",
                //         r => r.HasOne<UNIDADE_CURRICULAR>().WithMany()
                //             .HasForeignKey("uc_id")
                //             .HasConstraintName("FK__ALUNO_UC__uc_id__3493CFA7"),
                //         l => l.HasOne<ALUNO>().WithMany()
                //             .HasForeignKey("aluno_id")
                //             .OnDelete(DeleteBehavior.ClientSetNull)
                //             .HasConstraintName("FK__ALUNO_UC__aluno___339FAB6E"),
                //         j =>
                //         {
                //             j.HasKey("aluno_id", "uc_id").HasName("PK__ALUNO_UC__0378F9E125B17D91");
                //             j.ToTable("ALUNO_UC");
                //         });
            });

            // Configuração da Entidade ALUNO_UC
            modelBuilder.Entity<ALUNO_UC>(entity =>
            {
                entity.HasKey(e => new { e.aluno_id, e.uc_id })
                      .HasName("PK__ALUNO_UC__0378F9E125B17D91");

                entity.ToTable("ALUNO_UC");

                entity.Property(e => e.aluno_id).HasColumnName("aluno_id");
                entity.Property(e => e.uc_id).HasColumnName("uc_id");

                entity.HasOne(d => d.ALUNO)
                      .WithMany(p => p.ALUNO_UCs)
                      .HasForeignKey(d => d.aluno_id)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__ALUNO_UC__aluno___339FAB6E");

                entity.HasOne(d => d.UNIDADE_CURRICULAR)
                      .WithMany(p => p.ALUNO_UCs)
                      .HasForeignKey(d => d.uc_id)
                      .HasConstraintName("FK__ALUNO_UC__uc_id__3493CFA7");
            });

            modelBuilder.Entity<ANO_LETIVO>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__ANO_LETI__3213E83F565EF8AD");
            });

            modelBuilder.Entity<CALENDARIO_ACADEMICO>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__CALENDAR__3213E83F7DB74DB2");

                entity.HasOne(d => d.ano_letivo)
                      .WithMany(p => p.CALENDARIO_ACADEMICOs)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__CALENDARI__ano_l__0F624AF8");

                entity.HasOne(d => d.semana)
                      .WithMany(p => p.CALENDARIO_ACADEMICOs)
                      .OnDelete(DeleteBehavior.SetNull)
                      .HasConstraintName("FK__CALENDARI__seman__10566F31");
            });

            modelBuilder.Entity<COORDENADOR_CURSO>(entity =>
            {
                entity.HasKey(e => e.docente_id).HasName("PK__COORDENA__32BA4B10A6FC47E8");

                entity.Property(e => e.docente_id).ValueGeneratedNever();

                entity.HasOne(d => d.curso)
                      .WithMany(p => p.COORDENADOR_CURSOs)
                      .HasConstraintName("FK__COORDENAD__curso__2CF2ADDF");

                entity.HasOne(d => d.docente)
                      .WithOne(p => p.COORDENADOR_CURSO)
                      .HasConstraintName("FK__COORDENAD__docen__2BFE89A6");
            });

            modelBuilder.Entity<CURSO>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__CURSO__3213E83F1E85C08A");

                entity.HasOne(d => d.escola)
                      .WithMany(p => p.CURSOs)
                      .HasConstraintName("FK__CURSO__escola_id__619B8048");

                entity.HasOne(d => d.tipo_curso)
                      .WithMany(p => p.CURSOs)
                      .HasConstraintName("FK__CURSO__tipo_curs__60A75C0F");
            });

            modelBuilder.Entity<DOCENTE>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__DOCENTE__3213E83F28CAE0B0");

                entity.Property(e => e.id).ValueGeneratedNever();

                entity.HasOne(d => d.idNavigation)
                      .WithOne(p => p.DOCENTE)
                      .HasConstraintName("FK__DOCENTE__id__5CD6CB2B");

                entity.HasMany(d => d.periodo_horarios)
                      .WithMany(p => p.docentes)
                      .UsingEntity<Dictionary<string, object>>(
                          "INDISPONIBILIDADE_DOCENTE",
                          r => r.HasOne<PERIODO_HORARIO>()
                                .WithMany()
                                .HasForeignKey("periodo_horario_id")
                                .HasConstraintName("FK__INDISPONI__perio__245D67DE"),
                          l => l.HasOne<DOCENTE>()
                                .WithMany()
                                .HasForeignKey("docente_id")
                                .HasConstraintName("FK__INDISPONI__docen__236943A5"),
                          j =>
                          {
                              j.HasKey("docente_id", "periodo_horario_id")
                               .HasName("PK__INDISPON__7C4171043BA37F58");
                              j.ToTable("INDISPONIBILIDADE_DOCENTE");
                          });
            });

            modelBuilder.Entity<ESCOLA>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__ESCOLA__3213E83F731C77AB");

                entity.HasOne(d => d.instituicao)
                      .WithMany(p => p.ESCOLAs)
                      .HasConstraintName("FK_ESCOLA_INSTITUICAO");
            });

            modelBuilder.Entity<FUNCIONARIO>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__FUNCIONA__3213E83F82D8CF83");

                entity.Property(e => e.id).ValueGeneratedNever();

                entity.HasOne(d => d.idNavigation)
                      .WithOne(p => p.FUNCIONARIO)
                      .HasConstraintName("FK__FUNCIONARIO__id__7EF6D905");

                entity.HasOne(d => d.instituicao)
                      .WithMany(p => p.FUNCIONARIOs)
                      .HasConstraintName("FK__FUNCIONAR__insti__7FEAFD3E");
            });

            modelBuilder.Entity<HORARIO_REFERENCIAL>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__HORARIO___3213E83FE45D08D7");

                entity.HasOne(d => d.ano_letivo)
                      .WithMany(p => p.HORARIO_REFERENCIALs)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__HORARIO_R__ano_l__17F790F9");

                entity.HasOne(d => d.docente)
                      .WithMany(p => p.HORARIO_REFERENCIALs)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__HORARIO_R__docen__151B244E");

                entity.HasOne(d => d.periodo_horario)
                      .WithMany(p => p.HORARIO_REFERENCIALs)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__HORARIO_R__perio__17036CC0");

                entity.HasOne(d => d.sala)
                      .WithMany(p => p.HORARIO_REFERENCIALs)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__HORARIO_R__sala___160F4887");

                entity.HasOne(d => d.turma)
                      .WithMany(p => p.HORARIO_REFERENCIALs)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__HORARIO_R__turma__14270015");

                entity.HasOne(d => d.uc)
                      .WithMany(p => p.HORARIO_REFERENCIALs)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__HORARIO_R__uc_id__1332DBDC");
            });

            modelBuilder.Entity<HORARIO_SEMANAL>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__HORARIO___3213E83F9DDEF4CF");

                entity.HasOne(d => d.ano_letivo)
                      .WithMany(p => p.HORARIO_SEMANALs)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__HORARIO_S__ano_l__208CD6FA");

                entity.HasOne(d => d.docente)
                      .WithMany(p => p.HORARIO_SEMANALs)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__HORARIO_S__docen__1CBC4616");

                entity.HasOne(d => d.periodo_horario)
                      .WithMany(p => p.HORARIO_SEMANALs)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__HORARIO_S__perio__1EA48E88");

                entity.HasOne(d => d.sala)
                      .WithMany(p => p.HORARIO_SEMANALs)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__HORARIO_S__sala___1DB06A4F");

                entity.HasOne(d => d.semana)
                      .WithMany(p => p.HORARIO_SEMANALs)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__HORARIO_S__seman__1F98B2C1");

                entity.HasOne(d => d.turma)
                      .WithMany(p => p.HORARIO_SEMANALs)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__HORARIO_S__turma__1BC821DD");

                entity.HasOne(d => d.uc)
                      .WithMany(p => p.HORARIO_SEMANALs)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK__HORARIO_S__uc_id__1AD3FDA4");
            });

            modelBuilder.Entity<INSTITUICAO>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__INSTITUI__3213E83FF20C0F4B");
            });

            modelBuilder.Entity<PERIODO_HORARIO>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__PERIODO___3213E83F49C1BD39");
            });

            modelBuilder.Entity<SALA>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__SALA__3213E83F13588E39");

                entity.HasOne(d => d.escola)
                      .WithMany(p => p.SALAs)
                      .HasConstraintName("FK__SALA__escola_id__6B24EA82");

                entity.HasMany(d => d.periodo_horarios)
                      .WithMany(p => p.salas)
                      .UsingEntity<Dictionary<string, object>>(
                          "INDISPONIBILIDADE_SALA",
                          r => r.HasOne<PERIODO_HORARIO>()
                                .WithMany()
                                .HasForeignKey("periodo_horario_id")
                                .HasConstraintName("FK__INDISPONI__perio__282DF8C2"),
                          l => l.HasOne<SALA>()
                                .WithMany()
                                .HasForeignKey("sala_id")
                                .HasConstraintName("FK__INDISPONI__sala___2739D489"),
                          j =>
                          {
                              j.HasKey("sala_id", "periodo_horario_id")
                               .HasName("PK__INDISPON__B48EB5D3DDA4769F");
                              j.ToTable("INDISPONIBILIDADE_SALA");
                          });
            });

            modelBuilder.Entity<SEMANA>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__SEMANAS__3213E83F93C51456");

                entity.HasOne(d => d.ano_letivo)
                      .WithMany(p => p.SEMANAs)
                      .HasConstraintName("FK__SEMANAS__ano_let__0A9D95DB");
            });

            modelBuilder.Entity<TIPO_AULA>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__TIPO_AUL__3213E83FED8EF83C");
            });

            modelBuilder.Entity<TIPO_CURSO>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__TIPO_CUR__3213E83F8B557432");
            });

            modelBuilder.Entity<TIPO_UTILIZADOR>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__TIPO_UTI__3213E83F24E0000D");
            });

            modelBuilder.Entity<TURMA>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__TURMA__3213E83FA0C01996");

                entity.HasOne(d => d.docente)
                      .WithMany(p => p.TURMAs)
                      .HasConstraintName("FK__TURMA__docente_i__76969D2E");

                entity.HasOne(d => d.tipo_aula)
                      .WithMany(p => p.TURMAs)
                      .HasConstraintName("FK__TURMA__tipo_aula__778AC167");

                entity.HasOne(d => d.unidade_curricular)
                      .WithMany(p => p.TURMAs)
                      .HasConstraintName("FK__TURMA__unidade_c__75A278F5");
            });

            modelBuilder.Entity<UC_DOCENTE>(entity =>
            {
                entity.HasKey(e => new { e.uc_id, e.docente_id, e.tipo_aula_id })
                      .HasName("PK__UC_DOCEN__ACC3C4154869A555");

                entity.HasOne(d => d.docente)
                      .WithMany(p => p.UC_DOCENTEs)
                      .HasConstraintName("FK__UC_DOCENT__docen__7B5B524B");

                entity.HasOne(d => d.tipo_aula)
                      .WithMany(p => p.UC_DOCENTEs)
                      .HasConstraintName("FK__UC_DOCENT__tipo___7C4F7684");

                entity.HasOne(d => d.uc)
                      .WithMany(p => p.UC_DOCENTEs)
                      .HasConstraintName("FK__UC_DOCENT__uc_id__7A672E12");
            });

            modelBuilder.Entity<UNIDADE_CURRICULAR>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__UNIDADE___3213E83FF42A1072");

                entity.HasOne(d => d.curso)
                      .WithMany(p => p.UNIDADE_CURRICULARs)
                      .HasConstraintName("FK__UNIDADE_C__curso__72C60C4A");

                entity.HasMany(d => d.salas)
                      .WithMany(p => p.unidade_curriculars)
                      .UsingEntity<Dictionary<string, object>>(
                          "UC_SALA",
                          r => r.HasOne<SALA>()
                                .WithMany()
                                .HasForeignKey("sala_id")
                                .OnDelete(DeleteBehavior.ClientSetNull)
                                .HasConstraintName("FK__UC_SALA__sala_id__03F0984C"),
                          l => l.HasOne<UNIDADE_CURRICULAR>()
                                .WithMany()
                                .HasForeignKey("unidade_curricular_id")
                                .HasConstraintName("FK__UC_SALA__unidade__02FC7413"),
                          j =>
                          {
                              j.HasKey("unidade_curricular_id", "sala_id")
                               .HasName("PK__UC_SALA__6D7FA028C298D6E8");
                              j.ToTable("UC_SALA");
                          });
            });

            modelBuilder.Entity<UTILIZADOR>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PK__UTILIZAD__3213E83F5AD3497F");

                entity.HasMany(d => d.tipo_utilizadors)
                      .WithMany(p => p.utilizadors)
                      .UsingEntity<Dictionary<string, object>>(
                          "UTILIZADOR_TIPO_UTILIZADOR",
                          r => r.HasOne<TIPO_UTILIZADOR>()
                                .WithMany()
                                .HasForeignKey("tipo_utilizador_id")
                                .HasConstraintName("FK__UTILIZADO__tipo___59063A47"),
                          l => l.HasOne<UTILIZADOR>()
                                .WithMany()
                                .HasForeignKey("utilizador_id")
                                .HasConstraintName("FK__UTILIZADO__utili__5812160E"),
                          j =>
                          {
                              j.HasKey("utilizador_id", "tipo_utilizador_id")
                               .HasName("PK__UTILIZAD__765AED7DA60D76A1");
                              j.ToTable("UTILIZADOR_TIPO_UTILIZADOR");
                          });
            });

            modelBuilder.Entity<SolucaoHorario>()
                .HasMany(s => s.HorariosReferenciais)
                .WithOne(hr => hr.SolucaoHorario)
                .HasForeignKey(hr => hr.solucaoHorarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuração da Entidade ALUNO_UC já adicionada acima

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
