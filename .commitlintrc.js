module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'header-max-length': [2, 'always', 100],
    'type-enum': [
      2,
      'always',
      [
        'feat',
        'fix',
        'docs',
        'style',
        'refactor',
        'perf',
        'test',
        'chore',
        'revert',
        'ci',
        'build',
      ],
    ],
  },
  parserPreset: {
    parserOpts: {
      headerPattern: /^(:\w*:|(?:\ud83c[\udf00-\udfff])|(?:\ud83d[\udc00-\ude4f\ude80-\udeff])|[\u2600-\u26ff]\s)?(\w*)(\(([\w$.\-* ]*)\))?: (.*)$/,
      headerCorrespondence: ['emoji', 'type', 'scope', 'subject'],
    },
  },
};
