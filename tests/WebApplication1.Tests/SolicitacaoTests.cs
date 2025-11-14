using WebApplication1;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests
{
    public class SolicitacaoTests
    {
        [Fact]
        public void Setting_Status_sets_IdStatusSolicitacao()
        {
            var s = new Solicitacao();

            s.Status = StatusSolicitacaoEnum.EmAndamento;

            Assert.Equal(StatusSolicitacaoEnum.EmAndamento, s.Status);
        }

        [Fact]
        public void Setting_IdStatusSolicitacao_sets_Status()
        {
            var s = new Solicitacao
            {
                Status = StatusSolicitacaoEnum.Cancelado
            };

            Assert.Equal(StatusSolicitacaoEnum.Cancelado, s.Status);
        }

        [Fact]
        public void RoundTrip_all_enum_values_is_consistent()
        {
            foreach (StatusSolicitacaoEnum valor in Enum.GetValues(typeof(StatusSolicitacaoEnum)))
            {
                var s1 = new Solicitacao { Status = valor };
                Assert.Equal(valor, s1.Status);
                Assert.Equal(valor, s1.Status);

                var s2 = new Solicitacao { Status = valor };
                Assert.Equal(valor, s2.Status);
                Assert.Equal(valor, s2.Status);
            }
        }

        [Fact]
        public void Undefined_Id_casts_to_enum_value_even_if_not_defined()
        {
            var s = new Solicitacao { Status = (StatusSolicitacaoEnum)999 };

            Assert.Equal((StatusSolicitacaoEnum)999, s.Status);
        }
    }
}