# Entrega da funcionalidade de matrícula

Referência: `dev14e15de20.pdf`, principalmente páginas 2, 34, 35 e 36.
Prazo informado no material: **08/11**, para as duas turmas. O PDF não informa horário limite; confira a atividade do Classroom.

## Requisitos obrigatórios

- A solução completa deve estar no seu próprio repositório: https://github.com/HenriqueAgostinetto/AcademiaDoZe.
- A Dashboard deve mostrar a logo da aplicação com seu nome dentro da própria imagem. O arquivo `logo_henrique.png` já contém "Henrique Agostinetto Piva" e está referenciado na Dashboard.
- A identidade visual deve ser própria. O projeto mantém a paleta azul e dourado.
- A matrícula deve permitir aluno, plano, início, término automático, objetivo, múltiplas restrições, observações e imagem do laudo.
- Não pode ser cadastrada outra matrícula enquanto o aluno tiver matrícula ativa.
- O laudo é obrigatório para alunos menores de 16 anos e para alunos com restrições médicas, conforme o código exemplificado pelo professor.
- Não é necessário implementar controle financeiro para esta atividade.

O material alerta que a ausência do repositório próprio ou da logo com o nome na imagem zera o parcial.

## Preparação para a gravação

1. Abra o aplicativo e selecione um banco configurado e acessível. SQLite já tem inicialização automática ao abrir o aplicativo com esse banco selecionado.
2. Cadastre um logradouro, caso ainda não exista.
3. Cadastre um aluno com seu nome, CPF, nascimento e sua foto, se ele ainda não existir. Esse aluno não pode ter uma matrícula ativa antes da demonstração de cadastro.
4. Tenha uma imagem de laudo disponível para demonstrar as restrições médicas.
5. Grave a tela do computador com texto e imagem legíveis. O material não aceita vídeo com qualidade ruim e não define duração exata.

## Roteiro do vídeo

1. Mostre a Dashboard com a logo e seu nome na imagem.
2. Abra **Matrículas** e clique em **Nova matrícula**.
3. Busque pelo CPF ou selecione o aluno com seu nome. Mostre claramente nome, CPF, nascimento e miniatura da sua foto.
4. Escolha um plano e altere a data inicial para mostrar o cálculo automático da data final. As durações são 1, 3, 6 ou 12 meses.
5. Preencha o objetivo, marque pelo menos duas restrições médicas, registre observações e anexe a imagem do laudo.
6. Salve e mostre a matrícula na listagem. Anote o ID mostrado no cartão.
7. Clique em **Editar**, altere as restrições médicas e salve. Mostre as novas restrições na listagem.
8. Selecione o filtro **Id**, informe o ID e clique em **Buscar** para localizar a matrícula editada.
9. Clique em **Excluir**, confirme e mostre que a matrícula não aparece mais na busca.

O slide 36 repete cadastro, edição e exclusão; basta demonstrar o ciclo completo uma vez.

Antes de gravar, confira também as mensagens ao tentar salvar sem laudo quando ele é obrigatório e ao tentar criar uma segunda matrícula ativa.

## Arquivos para o Classroom

O resumo do slide 36 pede **três arquivos**:

1. **Texto com o endereço do Git.** O conteúdo está em `entrega/repositorio.txt`, pronto para anexar. O slide também menciona comentário, mas o arquivo texto atende ao resumo de três arquivos.
2. **ZIP da release criada no GitHub**, contendo a solução completa até esta etapa. O ZIP deve corresponder à versão corrigida enviada ao repositório.
3. **Vídeo da execução**, seguindo o roteiro acima.

Para preparar a release após revisar e enviar as alterações ao GitHub:

1. Na página do seu repositório, abra **Releases** e escolha **Draft a new release** ou **Create a new release**.
2. Crie uma tag para esta entrega, por exemplo `avaliacao02-matriculas`, apontando para o commit que contém as correções.
3. Informe um título e uma descrição da versão, e publique a release.
4. Baixe **Source code (zip)** nos arquivos da release. Confira se estão presentes a solução `.sln`, os projetos, o código e os recursos, incluindo a logo.
5. Anexe o ZIP, o texto e o vídeo na atividade correspondente do Classroom e finalize a entrega.

Não basta entregar somente a pasta do executável. O ZIP final deve ser baixado da release no GitHub, apontando para o commit com as correções. A publicação da release e a postagem no Classroom ainda precisam ser realizadas.

## Conferência técnica

- Foram incorporados os três commits que estavam no GitHub e faltavam no projeto local, até `2ec9d87`.
- Foram reutilizados os serviços, DTOs, MVVM, enums com `[Flags]` e conversores existentes.
- A listagem passou a mostrar CPF, nascimento, foto, objetivo, restrições e observações.
- Foi incluída a busca de aluno por CPF no cadastro de matrícula.
- O laudo pode ser selecionado como imagem ou capturado pela câmera, com tratamento de falhas.
- O teste de atualização passou a conferir a troca de múltiplas restrições, o laudo, as observações e o término anual.
- Validação: 124 testes de domínio e 47 de infraestrutura com SQLite, além da compilação dos aplicativos Windows e Android. SQL Server, MySQL e câmera não foram validados em execução nesta revisão.
- A detecção do Android no Windows foi ajustada para reconhecer o SDK instalado no caminho padrão, além das variáveis e configurações existentes.
- A compilação mantém avisos do CommunityToolkit sobre AOT/WinRT com `[ObservableProperty]`; a abordagem ensinada e já utilizada no projeto foi preservada.
- A abertura do aplicativo pelo controle automático do computador não foi autorizada pela ferramenta. Faça o ensaio do roteiro nas telas antes da gravação.

Há uma diferença no próprio material: a página 2 escreve "de 12 a 16 anos", enquanto os exemplos das páginas 12 e 18 usam idade menor que 16. O projeto segue a condição dos exemplos, sem incluir quem já completou 16 anos nessa regra por idade; restrições médicas continuam exigindo laudo em qualquer idade.
