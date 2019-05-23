const Generator = require('yeoman-generator');
const fs = require('fs');
const path = require('path');
const uuidv4 = require('uuid/v4');
const _ = require('lodash');


function readDir(dir) {
    return fs.statSync(dir).isDirectory() ? Array.prototype.concat(...fs.readdirSync(dir).map(f => readDir(path.join(dir, f)))) : dir;
}

module.exports = class extends Generator {

    constructor(args, opts) {
        super(args, opts);

        this.argument('shortModuleName', { type: String, required: true, description: 'Name of module to create' });

        this.options.projectId = uuidv4();
        this.options.moduleName = `Quark.Module.${this.options.shortModuleName}`;
        this.options.serviceName = _.camelCase(this.options.shortModuleName);
    }

    async prompting() {
        this.answers = await this.prompt([
            {
                type: 'list',
                name: 'moduleType',
                message: 'Choose module type',
                choices: [
                    { name: 'WebAPI module', value: 'webApiModule' },
                    { name: 'Minimal module', value: 'minimalModule' },
                ]
            },
            {
                type: 'confirm',
                name: 'serveStatic',
                message: 'Serve static files (SPA, for example)',
                default: false,
                when: function(currentAnswers) {
                    return currentAnswers.moduleType === 'webApiModule';
                }
            },
            {
                type: 'input',
                name: 'staticRoot',
                message: 'Static files root, relative to module deployment folder',
                default: 'app',
                when: function (currentAnswers) {
                    return !!currentAnswers.serveStatic;
                }
            },
            {
                type: 'confirm',
                name: 'handleSPA',
                message: 'Handle SPA redirection? (yes if you plan to host SPA as static module asset)',
                default: true,
                when: function (currentAnswers) {
                    return !!currentAnswers.serveStatic;
                }
            },
            {
                type: 'input',
                name: 'spaExemptPrefixes',
                message: 'URL prefixes (comma separated list if multiple) to be exempted from SPA redirection',
                default: '/api',
                when: function (currentAnswers) {
                    return !!currentAnswers.handleSPA;
                },
            },
            {
                type: 'confirm',
                name: 'apiKeyValidation',
                message: 'Use api key validation?',
                default: false,
                when: function(currentAnswers) {
                    return currentAnswers.moduleType === 'webApiModule';
                }
            },
        ]);
        if (this.answers.handleSPA === undefined) {
            this.answers.handleSPA = false;
        }
    }

    writing() {
        switch (this.answers.moduleType) {
            case 'webApiModule':
                this.private_create_module_files();
                break;
            case 'minimalModule':
                this.private_create_module_files();
                break;
        }
    }

    private_create_module_files() {

        const srcRoot = this.sourceRoot();
        const files = readDir(path.join(srcRoot, this.answers.moduleType))
            .map(p => p.substr(srcRoot.length + this.answers.moduleType.length + 2))
            .filter(f => path.extname(f) !== '.csproj')
            .map(f => path.normalize(f));

        // this.log(files);

        files
            .forEach(f => {
                this
                    .fs
                    .copyTpl(
                        this.templatePath(this.answers.moduleType, f),
                        this.destinationPath(this.options.moduleName, f),
                        { answers: this.answers, options: this.options }
                    );
            });

        // this.fs.commit(()=>{this.log('commited')});

        const projectFileName = `${this.options.moduleName}.csproj`;
        this
            .fs
            .copyTpl(
                this.templatePath(this.answers.moduleType, 'project.csproj'),
                this.destinationPath(this.options.moduleName, projectFileName),
                { answers: this.answers, options: this.options },
            );
    }

};