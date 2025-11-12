// src/hooks/useForm.ts
import { useState, useCallback } from 'react';

interface FormErrors {
    [key: string]: string;
}

interface UseFormOptions<T> {
    initialValues: T;
    validate?: (values: T) => FormErrors;
    onSubmit: (values: T) => Promise<void> | void;
}

const useForm = <T extends Record<string, any>>({
    initialValues,
    validate,
    onSubmit,
}: UseFormOptions<T>) => {
    const [values, setValues] = useState<T>(initialValues);
    const [errors, setErrors] = useState<FormErrors>({});
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setValues((prev) => ({
            ...prev,
            [name]: value,
        }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (validate) {
            const validationErrors = validate(values);
            setErrors(validationErrors);

            if (Object.keys(validationErrors).length > 0) {
                return;
            }
        }

        setIsSubmitting(true);
        try {
            await onSubmit(values);
        } finally {
            setIsSubmitting(false);
        }
    };

    const resetForm = useCallback(() => {
        setValues(initialValues);
        setErrors({});
    }, [initialValues]);

    return {
        values,
        errors,
        isSubmitting,
        handleChange,
        handleSubmit,
        resetForm,
        setValues,
        setErrors,
    };
};

export default useForm;