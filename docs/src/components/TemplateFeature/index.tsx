import type {ReactNode} from 'react';
import {useDoc} from '@docusaurus/plugin-content-docs/client';
import Admonition from '@theme/Admonition';

type TemplateFeature = {
    name: string;
    version: string;
    isExperimental: boolean;
    website: string;
    files: string[];
}

function useFeature(): TemplateFeature | null {
    const {frontMatter} = useDoc();
    const template = (frontMatter as {template?: {feature?: TemplateFeature}}).template;
    return template?.feature ?? null;
}

export function TemplateFeatureWebsite(): ReactNode {
    const feature = useFeature();
    if (feature?.website) {
        return <Admonition type="info" title="Website">
            <a href={feature.website}>{feature.website}</a>
        </Admonition>;
    }
    throw new Error('Feature website not found in front matter.');
}

export function TemplateFeatureFiles(): ReactNode {
    const feature = useFeature();
    if (feature?.files) {
        return (
            <ul>
                {feature.files.map((file) => (
                    <li key={file}><code>{file}</code></li>
                ))}
            </ul>
        );
    }
    throw new Error('Feature files not found in front matter.');
}
