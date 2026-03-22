export type Language = 'en' | 'pt';

export interface Translations {
  tagline: string;
  description: string;
  beginButton: string;
  consultingOracle: string;
  footer: string;
  answerYes: string;
  answerProbably: string;
  answerIDontKnow: string;
  answerProbablyNot: string;
  answerNo: string;
  readingMinds: string;
  questionProgress: string;
  questionOf: (current: number, max: number) => string;
  oracleDeclares: string;
  wasIRight: string;
  guessYes: string;
  guessNo: string;
  oracleKnowsAll: string;
  oraclePrevails: string;
  mindRead: string;
  oracleStumped: string;
  tooMysterious: string;
  admitsDefeat: string;
  playAgain: string;
  hint: string;
  correctAnswerPrompt: string;
  correctAnswerPlaceholder: string;
  submitCorrection: string;
  skipCorrection: string;
}

export const translations: Record<Language, Translations> = {
  en: {
    tagline: 'The Oracle Knows All',
    description:
      'Think of anything — a person, animal, place, food, or fictional character. The Oracle will read your mind in 20 questions or less.',
    beginButton: 'Begin the Reading',
    consultingOracle: 'Consulting the Oracle...',
    footer: 'Up to 20 questions · Powered by Claude AI',
    answerYes: 'Yes',
    answerProbably: 'Probably',
    answerIDontKnow: "I don't know",
    answerProbablyNot: 'Probably not',
    answerNo: 'No',
    readingMinds: 'Reading minds...',
    questionProgress: 'Question Progress',
    questionOf: (current, max) => `Question ${current} of ${max}`,
    oracleDeclares: 'The Oracle declares...',
    wasIRight: 'Was I right?',
    guessYes: 'Yes!',
    guessNo: 'No',
    oracleKnowsAll: 'The Oracle Knows All',
    oraclePrevails: 'The Oracle Prevails!',
    mindRead: 'Your mind has been read. The Oracle sees all.',
    oracleStumped: 'The Oracle is Stumped',
    tooMysterious: 'Your mind proved too mysterious this time.',
    admitsDefeat: 'After 20 questions, the Oracle admits defeat.',
    playAgain: 'Play Again',
    hint: "Think of your answer, then choose the option that best describes it. The Oracle will deduce what you're thinking.",
    correctAnswerPrompt: 'What were you thinking of?',
    correctAnswerPlaceholder: 'Enter the correct answer...',
    submitCorrection: 'Submit',
    skipCorrection: 'Skip',
  },

  pt: {
    tagline: 'O Oráculo Tudo Sabe',
    description:
      'Pense em qualquer coisa — uma pessoa, animal, lugar, comida ou personagem fictício. O Oráculo lerá sua mente em 20 perguntas ou menos.',
    beginButton: 'Iniciar a Leitura',
    consultingOracle: 'Consultando o Oráculo...',
    footer: 'Até 20 perguntas · Desenvolvido com Claude AI',
    answerYes: 'Sim',
    answerProbably: 'Provavelmente',
    answerIDontKnow: 'Não sei',
    answerProbablyNot: 'Provavelmente não',
    answerNo: 'Não',
    readingMinds: 'Lendo mentes...',
    questionProgress: 'Progresso',
    questionOf: (current, max) => `Pergunta ${current} de ${max}`,
    oracleDeclares: 'O Oráculo declara...',
    wasIRight: 'Acertei?',
    guessYes: 'Sim!',
    guessNo: 'Não',
    oracleKnowsAll: 'O Oráculo Tudo Sabe',
    oraclePrevails: 'O Oráculo Venceu!',
    mindRead: 'Sua mente foi lida. O Oráculo tudo vê.',
    oracleStumped: 'O Oráculo foi Derrotado',
    tooMysterious: 'Sua mente foi misteriosa demais desta vez.',
    admitsDefeat: 'Após 20 perguntas, o Oráculo admite a derrota.',
    playAgain: 'Jogar Novamente',
    hint: 'Pense na sua resposta e escolha a opção que melhor a descreve. O Oráculo deduzirá o que você está pensando.',
    correctAnswerPrompt: 'O que você estava pensando?',
    correctAnswerPlaceholder: 'Digite a resposta correta...',
    submitCorrection: 'Enviar',
    skipCorrection: 'Pular',
  },
};
