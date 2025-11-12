// src/components/FormInput.tsx
import React from 'react';
import {
    TextField,
    TextFieldProps,
    FormControl,
    FormHelperText,
    InputLabel,
    Select,
    MenuItem,
    SelectProps,
} from '@mui/material';

type FormInputProps = TextFieldProps & {
    name: string;
    label: string;
    errorText?: string;
    select?: boolean;
    options?: Array<{ value: string | number; label: string }>;
    selectProps?: SelectProps;
};

const FormInput: React.FC<FormInputProps> = ({
    name,
    label,
    errorText,
    select = false,
    options = [],
    selectProps,
    fullWidth = true,
    margin = 'normal',
    variant = 'outlined',
    ...props
}) => {
    if (select) {
        return (
            <FormControl
                fullWidth={fullWidth}
                margin={margin}
                variant={variant as 'outlined' | 'standard' | 'filled'}
                error={!!errorText}
            >
                <InputLabel id={`${name}-label`}>{label}</InputLabel>
                <Select
                    labelId={`${name}-label`}
                    id={name}
                    name={name}
                    label={label}
                    variant={variant as 'outlined' | 'standard' | 'filled'}
                    {...selectProps}
                    {...props as any}
                >
                    {options.map((option) => (
                        <MenuItem key={option.value} value={option.value}>
                            {option.label}
                        </MenuItem>
                    ))}
                </Select>
                {errorText && <FormHelperText>{errorText}</FormHelperText>}
            </FormControl>
        );
    }

    return (
        <TextField
            name={name}
            label={label}
            fullWidth={fullWidth}
            margin={margin}
            variant={variant}
            helperText={errorText}
            error={!!errorText}
            {...props}
        />
    );
};

export default FormInput;